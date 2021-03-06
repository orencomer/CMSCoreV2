﻿using CMSCoreV2.Data;
using CMSCoreV2.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using SaasKit.Multitenancy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CMSCoreV2
{
    public class CachingAppTenantResolver : MemoryCacheTenantResolver<AppTenant>
    {
        private readonly HostDbContext _dbContext;
        public CachingAppTenantResolver(HostDbContext dbContext, IMemoryCache cache, ILoggerFactory loggerFactory)
            : base(cache, loggerFactory)
        {
            _dbContext = dbContext;
        }

        protected override string GetContextIdentifier(HttpContext context)
        {
            return context.Request.Host.Value.ToLower();
        }

        protected override IEnumerable<string> GetTenantIdentifiers(TenantContext<AppTenant> context)
        {
            return new[] { context.Tenant.Hostname };
        }

        protected override Task<TenantContext<AppTenant>> ResolveAsync(HttpContext context)
        {
            TenantContext<AppTenant> tenantContext = null;
            var hostName = context.Request.Host.Value.ToLower();

            var tenant = _dbContext.AppTenants.FirstOrDefault(
                t => t.Hostname.Equals(hostName));

            if (tenant != null)
            {
                tenantContext = new TenantContext<AppTenant>(tenant);
            }

            return Task.FromResult(tenantContext);
        }
    }
}
