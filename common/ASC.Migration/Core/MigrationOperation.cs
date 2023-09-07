﻿// (c) Copyright Ascensio System SIA 2010-2022
//
// This program is a free software product.
// You can redistribute it and/or modify it under the terms
// of the GNU Affero General Public License (AGPL) version 3 as published by the Free Software
// Foundation. In accordance with Section 7(a) of the GNU AGPL its Section 15 shall be amended
// to the effect that Ascensio System SIA expressly excludes the warranty of non-infringement of
// any third-party rights.
//
// This program is distributed WITHOUT ANY WARRANTY, without even the implied warranty
// of MERCHANTABILITY or FITNESS FOR A PARTICULAR  PURPOSE. For details, see
// the GNU AGPL at: http://www.gnu.org/licenses/agpl-3.0.html
//
// You can contact Ascensio System SIA at Lubanas st. 125a-25, Riga, Latvia, EU, LV-1021.
//
// The  interactive user interfaces in modified source and object code versions of the Program must
// display Appropriate Legal Notices, as required under Section 5 of the GNU AGPL version 3.
//
// Pursuant to Section 7(b) of the License you must retain the original Product logo when
// distributing the program. Pursuant to Section 7(e) we decline to grant you any rights under
// trademark law for use of our trademarks.
//
// All the Product's GUI elements, including illustrations and icon sets, as well as technical writing
// content are licensed under the terms of the Creative Commons Attribution-ShareAlike 4.0
// International. See the License terms at http://creativecommons.org/licenses/by-sa/4.0/legalcode

namespace ASC.Migration.Core;

[Transient]
public class MigrationOperation : DistributedTaskProgress
{
    private readonly ILogger<MigrationOperation> _logger;
    private readonly MigrationCore _migrationCore;
    private readonly TenantManager _tenantManager;
    private readonly SecurityContext _securityContext;
    private readonly StorageFactory _storageFactory;
    private readonly IServiceProvider _serviceProvider;
    private string _migratorName;
    private Guid _userId;

    private int? _tenantId;
    public int TenantId
    {
        get => _tenantId ?? this[nameof(_tenantId)];
        set
        {
            _tenantId = value;
            this[nameof(_tenantId)] = value;
        }
    }

    private MigrationApiInfo _migrationApiInfo;
    public MigrationApiInfo MigrationApiInfo
    {
        get => _migrationApiInfo ?? System.Text.Json.JsonSerializer.Deserialize<MigrationApiInfo>(this[nameof(_migrationApiInfo)]);
        set
        {
            _migrationApiInfo = value;
            this[nameof(_migrationApiInfo)] = System.Text.Json.JsonSerializer.Serialize(value);
        }
    }

    private List<Guid> _importedUsers;
    public List<Guid> ImportedUsers
    {
        get => _migrationApiInfo ?? System.Text.Json.JsonSerializer.Deserialize<List<Guid>>(this[nameof(_importedUsers)]);
        set
        {
            _importedUsers = value;
            this[nameof(_importedUsers)] = System.Text.Json.JsonSerializer.Serialize(value);
        }
    }

    private string _logName;
    public string LogName
    {
        get => _logName ?? this[nameof(_logName)];
        set
        {
            _logName = value;
            this[nameof(_logName)] = value;
        }
    }

    public MigrationOperation(
        ILogger<MigrationOperation> logger,
        MigrationCore migrationCore,
        TenantManager tenantManager,
        SecurityContext securityContext,
        IServiceProvider serviceProvider,
        StorageFactory storageFactory)
    {
        _logger = logger;
        _migrationCore = migrationCore;
        _tenantManager = tenantManager;
        _securityContext = securityContext;
        _serviceProvider = serviceProvider;
        _storageFactory = storageFactory;
    }

    public void InitParse(int tenantId, Guid userId, string migratorName)
    {
        TenantId = tenantId;
        _migratorName = migratorName;
        _userId = userId;
    }

    public void InitMigrate(int tenantId, Guid userId, MigrationApiInfo migrationApiInfo)
    {
        TenantId = tenantId;
        MigrationApiInfo = migrationApiInfo;
        _migratorName = migrationApiInfo.MigratorName;
        _userId = userId;
    }

    protected override async Task DoJob()
    {
        IMigration migrator = null;
        try
        {
            CustomSynchronizationContext.CreateContext();

            await _tenantManager.SetCurrentTenantAsync(TenantId);
            await _securityContext.AuthenticateMeWithoutCookieAsync(_userId);
            migrator = _migrationCore.GetMigrator(_migratorName);
            migrator.OnProgressUpdate += Migrator_OnProgressUpdate;

            if (migrator == null)
            {
                throw new ItemNotFoundException(MigrationResource.MigrationNotFoundException);
            }

            try
            {
                var discStore = await _storageFactory.GetStorageAsync(TenantId, "migration", (IQuotaController)null) as DiscDataStore;
                var folder = discStore.GetPhysicalPath("", "");
                migrator.Init(folder, CancellationToken);

            }
            catch (Exception ex)
            {
                throw new Exception(string.Format(MigrationResource.MigrationUploadException, _migratorName), ex);
            }

            var onlyParse = _migrationApiInfo == null;
            await migrator.Parse(onlyParse);

            if (!onlyParse)
            {
                await migrator.Migrate(_migrationApiInfo);
            }
        }
        catch (Exception e)
        {
            Exception = e;
            _logger.ErrorWithException(e);
        }
        finally
        {
            IsCompleted = true;
            if (migrator != null)
            {
                migrator.OnProgressUpdate -= Migrator_OnProgressUpdate;
                ImportedUsers = migrator.GetGuidImportedUsers();
                LogName = migrator.GetLogName();
                if(_migrationApiInfo is null)
                {
                    MigrationApiInfo = new MigrationApiInfo();
                }
            }

            PublishChanges();
            migrator.Dispose();
        }

        void Migrator_OnProgressUpdate(double arg1, string arg2)
        {
            Percentage = arg1;
            if (migrator != null && migrator.ApiInfo != null)
            {
                MigrationApiInfo = migrator.ApiInfo;
            }
            PublishChanges();
        }
    }

    public async Task CopyLogsAsync(Stream stream)
    {
        using var logger = _serviceProvider.GetService<MigrationLogger>();
        logger.Init(LogName);
        await logger.GetStream().CopyToAsync(stream);
    }
}