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

using AutoMapper;

namespace ASC.Webhooks.Core;

[Scope]
public class DbWorker
{
    private readonly IDbContextFactory<WebhooksDbContext> _dbContextFactory;
    private readonly TenantManager _tenantManager;
    private readonly AuthContext _authContext;
    private readonly IMapper _mapper;

    private int Tenant
    {
        get
        {
            return _tenantManager.GetCurrentTenant().Id;
        }
    }

    public DbWorker(
        IDbContextFactory<WebhooksDbContext> dbContextFactory,
        TenantManager tenantManager,
        AuthContext authContext,
        IMapper mapper)
    {
        _dbContextFactory = dbContextFactory;
        _tenantManager = tenantManager;
        _authContext = authContext;
        _mapper = mapper;
    }

    public async Task<WebhooksConfig> AddWebhookConfig(string uri, string name, string secretKey)
    {
        using var webhooksDbContext = _dbContextFactory.CreateDbContext();

        var objForCreate = await webhooksDbContext.WebhooksConfigs
           .Where(it => it.TenantId == Tenant && it.Uri == uri && it.Name == name)
           .FirstOrDefaultAsync();

        if (objForCreate != null)
        {
            return objForCreate;
        }

        var toAdd = new WebhooksConfig
        {
            TenantId = Tenant,
            Uri = uri,
            SecretKey = secretKey,
            Name = name
        };

        toAdd = await webhooksDbContext.AddOrUpdateAsync(r => r.WebhooksConfigs, toAdd);
        await webhooksDbContext.SaveChangesAsync();

        return toAdd;
    }

    public async IAsyncEnumerable<WebhooksConfigWithStatus> GetTenantWebhooksWithStatus()
    {
        using var webhooksDbContext = _dbContextFactory.CreateDbContext();

        var q =  webhooksDbContext.WebhooksConfigs
            .AsNoTracking()
            .Where(it => it.TenantId == Tenant)
            .GroupJoin(webhooksDbContext.WebhooksLogs, c => c.Id, l => l.ConfigId, (configs, logs) => new { configs, logs })
            .Select(it =>
                new WebhooksConfigWithStatus
                {
                    WebhooksConfig = it.configs,
                    Status = it.logs.OrderBy(it => it.Delivery).LastOrDefault().Status
                })
            .AsAsyncEnumerable();

        await foreach (var webhook in q)
        {
            yield return webhook;
        }
    }

    public IAsyncEnumerable<WebhooksConfig> GetWebhookConfigs()
    {
        var webhooksDbContext = _dbContextFactory.CreateDbContext();

        return webhooksDbContext.WebhooksConfigs
            .Where(t => t.TenantId == Tenant)
            .AsAsyncEnumerable();
    }

    public async Task<WebhooksConfig> UpdateWebhookConfig(int id, string name, string uri, string key, bool? enabled, bool? ssl)
    {
        using var webhooksDbContext = _dbContextFactory.CreateDbContext();

        var updateObj = await webhooksDbContext.WebhooksConfigs
            .Where(it => it.TenantId == Tenant && it.Id == id)
            .FirstOrDefaultAsync();

        if (updateObj != null)
        {
            if (!string.IsNullOrEmpty(name))
            {
                updateObj.Name = name;
            }

            if (!string.IsNullOrEmpty(uri))
            {
                updateObj.Uri = uri;
            }

            if (!string.IsNullOrEmpty(key))
            {
                updateObj.SecretKey = key;
            }

            if (enabled.HasValue)
            {
                updateObj.Enabled = enabled.Value;
            }

            if (ssl.HasValue)
            {
                updateObj.SSL = ssl.Value;
            }

            webhooksDbContext.WebhooksConfigs.Update(updateObj);
            await webhooksDbContext.SaveChangesAsync();
        }

        return updateObj;
    }

    public async Task<WebhooksConfig> RemoveWebhookConfig(int id)
    {
        var tenant = _tenantManager.GetCurrentTenant().Id;

        using var webhooksDbContext = _dbContextFactory.CreateDbContext();

        var removeObj = await webhooksDbContext.WebhooksConfigs
            .Where(it => it.TenantId == tenant && it.Id == id)
            .FirstOrDefaultAsync();

        if (removeObj != null)
        {
            webhooksDbContext.WebhooksConfigs.Remove(removeObj);
            await webhooksDbContext.SaveChangesAsync();
        }

        return removeObj;
    }

    public IAsyncEnumerable<WebhooksLog> ReadJournal(int startIndex, int limit, DateTime? deliveryFrom, DateTime? deliveryTo, string hookUri, int? hookId, int? configId, WebhookGroupStatus? webhookGroupStatus)
    {
        var webhooksDbContext = _dbContextFactory.CreateDbContext();

        var q = webhooksDbContext.WebhooksLogs
            .AsNoTracking()
            .Where(r => r.TenantId == Tenant);

        if (deliveryFrom.HasValue)
        {
            var from = deliveryFrom.Value;
            q = q.Where(r => r.Delivery >= from);
        }

        if (deliveryTo.HasValue)
        {
            var to = deliveryTo.Value;
            q = q.Where(r => r.Delivery <= to);
        }

        if (!string.IsNullOrEmpty(hookUri))
        {
            q = q.Where(r => r.Config.Uri == hookUri);
        }

        if (hookId != null)
        {
            q = q.Where(r => r.WebhookId == hookId);
        }

        if (configId != null)
        {
            q = q.Where(r => r.ConfigId == configId);
        }

        if(webhookGroupStatus != null)
        {
            if ((webhookGroupStatus & WebhookGroupStatus.NotSent) != WebhookGroupStatus.NotSent)
            {
                q = q.Where(r => r.Status != 0);
            }
            if ((webhookGroupStatus & WebhookGroupStatus.Status2xx) != WebhookGroupStatus.Status2xx)
            {
                q = q.Where(r => r.Status < 200 && r.Status >= 300);
            }
            if ((webhookGroupStatus & WebhookGroupStatus.Status3xx) != WebhookGroupStatus.Status3xx)
            {
                q = q.Where(r => r.Status < 300 && r.Status >= 400);
            }
            if ((webhookGroupStatus & WebhookGroupStatus.Status4xx) != WebhookGroupStatus.Status4xx)
            {
                q = q.Where(r => r.Status < 400 && r.Status >= 500);
            }
            if ((webhookGroupStatus & WebhookGroupStatus.Status5xx) != WebhookGroupStatus.Status5xx)
            {
                q = q.Where(r => r.Status < 500);
            }
        }

        if (startIndex != 0)
        {
            q = q.Skip(startIndex);
        }

        if (limit != 0)
        {
            q = q.Take(limit);
        }

        return q.OrderByDescending(t => t.Id).AsAsyncEnumerable();
    }

    public async Task<WebhooksLog> ReadJournal(int id)
    {
        using var webhooksDbContext = _dbContextFactory.CreateDbContext();

        return await webhooksDbContext.WebhooksLogs
            .AsNoTracking()
            .Where(it => it.Id == id)
            .FirstOrDefaultAsync();
    }

    public async Task<WebhooksLog> WriteToJournal(WebhooksLog webhook)
    {
        webhook.TenantId = _tenantManager.GetCurrentTenant().Id;
        webhook.Uid = _authContext.CurrentAccount.ID;

        using var webhooksDbContext = _dbContextFactory.CreateDbContext();

        var entity = await webhooksDbContext.WebhooksLogs.AddAsync(webhook);
        await webhooksDbContext.SaveChangesAsync();

        return entity.Entity;
    }

    public async Task<WebhooksLog> UpdateWebhookJournal(int id, int status, DateTime delivery, string requestHeaders, string responsePayload, string responseHeaders)
    {
        using var webhooksDbContext = _dbContextFactory.CreateDbContext();

        var webhook = await webhooksDbContext.WebhooksLogs.Where(t => t.Id == id).FirstOrDefaultAsync();
        webhook.Status = status;
        webhook.RequestHeaders = requestHeaders;
        webhook.ResponsePayload = responsePayload;
        webhook.ResponseHeaders = responseHeaders;
        webhook.Delivery = delivery;

        webhooksDbContext.WebhooksLogs.Update(webhook);
        await webhooksDbContext.SaveChangesAsync();

        return webhook;
    }

    public async Task Register(List<Webhook> webhooks)
    {
        using var webhooksDbContext = _dbContextFactory.CreateDbContext();

        var dbWebhooks = await webhooksDbContext.Webhooks.ToListAsync();

        foreach (var webhook in webhooks)
        {
            if (!dbWebhooks.Any(r => r.Route == webhook.Route && r.Method == webhook.Method))
            {
                try
                {
                    await webhooksDbContext.Webhooks.AddAsync(_mapper.Map<DbWebhook>(webhook));
                    await webhooksDbContext.SaveChangesAsync();
                }
                catch (Exception)
                {

                }
            }
        }
    }

    public async Task<List<Webhook>> GetWebhooksAsync()
    {
        using var webhooksDbContext = _dbContextFactory.CreateDbContext();
        var webHooks = await webhooksDbContext.Webhooks.AsNoTracking().ToListAsync();
        return _mapper.Map<List<DbWebhook>, List<Webhook>>(webHooks);
    }

    public async Task<Webhook> GetWebhookAsync(int id)
    {
        using var webhooksDbContext = _dbContextFactory.CreateDbContext();
        var webHook = await webhooksDbContext.Webhooks.Where(r => r.Id == id).AsNoTracking().FirstOrDefaultAsync();
        return _mapper.Map<DbWebhook, Webhook>(webHook);
    }

    public async Task<Webhook> GetWebhookAsync(string method, string routePattern)
    {
        using var webhooksDbContext = _dbContextFactory.CreateDbContext();

        var webHook = await webhooksDbContext.Webhooks
            .Where(r => r.Method == method && r.Route == routePattern)
            .AsNoTracking()
            .FirstOrDefaultAsync();

        return _mapper.Map<DbWebhook, Webhook>(webHook);
    }
}

[Flags]
public enum WebhookGroupStatus
{
    NotSent = 0x0,
    Status2xx = 0x1,
    Status3xx = 0x2,
    Status4xx = 0x4,
    Status5xx = 0x8
}