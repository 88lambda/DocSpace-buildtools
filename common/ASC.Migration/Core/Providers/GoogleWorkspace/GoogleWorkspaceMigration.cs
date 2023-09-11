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



namespace ASC.Migration.GoogleWorkspace;

[Scope]
public class GoogleWorkspaceMigration : AbstractMigration<GwsMigrationInfo, GwsMigratingUser, GwsMigratingFiles>
{
    private string[] _takeouts;
    private readonly SecurityContext _securityContext;
    private readonly UserManager _userManager;
    private readonly TempPath _tempPath;
    private readonly IServiceProvider _serviceProvider;
    private readonly MigratorMeta _meta;
    public override MigratorMeta Meta => _meta;

    public GoogleWorkspaceMigration(
        MigrationLogger migrationLogger,
        SecurityContext securityContext,
        TempPath tempPath,
        IServiceProvider serviceProvider,
        UserManager userManager)
        : base(migrationLogger)
    {
        _securityContext = securityContext;
        _tempPath = tempPath;
        _serviceProvider = serviceProvider;
        _meta = new("GoogleWorkspace", 5, true);
        _userManager = userManager;
    }

    public override void Init(string path, CancellationToken cancellationToken)
    {
        _logger.Init();
        _cancellationToken = cancellationToken;
        var tempTakeouts = new List<string>();
        var files = Directory.GetFiles(path);
        if (!files.Any() || !files.Any(f => f.EndsWith(".zip")))
        {
            throw new Exception("Folder must not be empty and should contain .zip files.");
        }
        foreach (var item in files)
        {
            if (item.EndsWith(".zip"))
            {
                tempTakeouts.Add(item);
            }
        }
        _takeouts = tempTakeouts.ToArray();

        _migrationInfo = new GwsMigrationInfo();
        _migrationInfo.MigratorName = _meta.Name;
        _migrationInfo.Path = path;
    }

    public override async Task<MigrationApiInfo> Parse(bool reportProgress = true)
    {
        if (reportProgress)
        {
            ReportProgress(5, MigrationResource.StartOfDataProcessing);
        }

        var progressStep = 90 / _takeouts.Length;
        var i = 1;
        foreach (var takeout in _takeouts)
        {
            if (_cancellationToken.IsCancellationRequested)
            {
                if (reportProgress)
                {
                    ReportProgress(100, MigrationResource.MigrationCanceled);
                }
                return null;
            }

            if (reportProgress)
            {
                ReportProgress(GetProgress() + progressStep, MigrationResource.DataProcessing + $" {takeout} ({i++}/{_takeouts.Length})");
            }

            var tmpFolder = Path.Combine(_tempPath.GetTempPath(), Path.GetFileNameWithoutExtension(takeout)); 
            var key = Path.GetFileName(takeout);
            try
            {
                ZipFile.ExtractToDirectory(takeout, tmpFolder);
                var rootFolder = Path.Combine(tmpFolder, "Takeout");

                if (!Directory.Exists(rootFolder))
                {
                    throw new Exception("Takeout zip does not contain root 'Takeout' folder.");
                }
                var directories = Directory.GetDirectories(rootFolder);
                var user = _serviceProvider.GetService<GwsMigratingUser>();
                    user.Init(key, rootFolder, Log);
                    user.Parse();
                    if (user.Email.IsNullOrEmpty()) 
                    {
                        _migrationInfo.WithoutEmailUsers.Add(key, user);
                    }
                    else if((await _userManager.GetUserByEmailAsync(user.Email)) != ASC.Core.Users.Constants.LostUser)
                    {
                        _migrationInfo.ExistUsers.Add(key, user);
                    }
                    else
                    {
                        _migrationInfo.Users.Add(key, user);
                    }
            }
            catch (Exception ex)
            {
                _migrationInfo.FailedArchives.Add(key);
                Log($"Couldn't parse user from {key} archive", ex);
            }
            finally
            {
                if (Directory.Exists(tmpFolder))
                {
                    Directory.Delete(tmpFolder, true);
                }
            }
        }
        if (reportProgress)
        {
            ReportProgress(100, MigrationResource.DataProcessingCompleted);
        }

        return _migrationInfo.ToApiInfo();
    }

    public override async Task Migrate(MigrationApiInfo migrationApiInfo)
    {
        ReportProgress(0, MigrationResource.PreparingForMigration);
        _importedUsers = new List<Guid>();
        _migrationInfo.Merge(migrationApiInfo);

        var usersForImport = _migrationInfo.Users
            .Where(u => u.Value.ShouldImport)
            .Select(u => u.Value);

        var failedUsers = new List<GwsMigratingUser>();
        var usersCount = usersForImport.Count();
        var progressStep = 25 / usersCount;
        // Add all users first
        var i = 1;
        foreach (var u in usersForImport)
        {
            if (_cancellationToken.IsCancellationRequested) { ReportProgress(100, MigrationResource.MigrationCanceled); return; }

            var user = _serviceProvider.GetService<GwsMigratingUser>();
            user.Init(u);

            ReportProgress(GetProgress() + progressStep, string.Format(MigrationResource.UserMigration, user.DisplayName, i++, usersCount));
            try
            {
                var elem = migrationApiInfo.Users.Find(element => element.Key == user.Key);
                user.DataСhange(elem);
                user.UserType = elem.UserType;

                await user.MigrateAsync();
                _importedUsers.Add(user.Guid);
            }
            catch (Exception ex)
            {
                failedUsers.Add(user);
                Log($"Couldn't migrate user {user.DisplayName} ({user.Email})", ex);
            }
        }

        // Add files, contacts and other stuff
        i = 1;
        foreach (var user in usersForImport)
        {
            if (_cancellationToken.IsCancellationRequested) { ReportProgress(100, MigrationResource.MigrationCanceled); return; }
            if (failedUsers.Contains(user))
            {
                ReportProgress(GetProgress() + progressStep, string.Format(MigrationResource.UserSkipped, user.DisplayName, i, usersCount));
                continue;
            }

            var smallStep = progressStep / 4;

            try
            {
                var currentUser = _securityContext.CurrentAccount;
                await _securityContext.AuthenticateMeAsync(user.Guid);
                user.MigratingFiles.SetUsersDict(usersForImport.Except(failedUsers));
                await user.MigratingFiles.MigrateAsync();
                await _securityContext.AuthenticateMeAsync(currentUser.ID);
            }
            catch (Exception ex)
            {
                Log($"Couldn't migrate user {user.DisplayName} ({user.Email}) files", ex);
            }
            finally
            {
                ReportProgress(GetProgress() + smallStep, string.Format(MigrationResource.MigratingUserFiles, user.DisplayName, i, usersCount));
            }
            i++;
        }

        foreach (var item in _takeouts)
        {
            File.Delete(item);
        }

        ReportProgress(100, MigrationResource.MigrationCompleted);
    }
}
