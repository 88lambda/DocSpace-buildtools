﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq.Expressions;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ASC.Core.Common.EF
{
    public enum Provider
    {
        Postrge,
        MySql
    }

    public class BaseDbContext : DbContext
    {
        public string baseName;
        public BaseDbContext() { }
        public BaseDbContext(DbContextOptions options) : base(options)
        {
            
        }

        internal ILoggerFactory LoggerFactory { get; set; }
        internal ConnectionStringSettings ConnectionStringSettings { get; set; }
        internal Provider Provider { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseLoggerFactory(LoggerFactory);
            optionsBuilder.EnableSensitiveDataLogging();
            switch (ConnectionStringSettings.ProviderName)
            {
                case "MySql.Data.MySqlClient":
                    Provider = Provider.MySql;
                    optionsBuilder.UseMySql(ConnectionStringSettings.ConnectionString);
                    break;
                case "Npgsql":
                    Provider = Provider.Postrge;
                    optionsBuilder.UseNpgsql(ConnectionStringSettings.ConnectionString);
                    break;
            }
        }
    }

    public static class BaseDbContextExtension
    {
        public static T AddOrUpdate<T, TContext>(this TContext b, Expression<Func<TContext, DbSet<T>>> expressionDbSet, T entity) where T : BaseEntity where TContext : BaseDbContext
        {
            var dbSet = expressionDbSet.Compile().Invoke(b);
            var existingBlog = dbSet.Find(entity.GetKeys());
            if (existingBlog == null)
            {
                return dbSet.Add(entity).Entity;
            }
            else
            {
                b.Entry(existingBlog).CurrentValues.SetValues(entity);
                return entity;
            }
        }
    }

    public abstract class BaseEntity
    {
        public abstract object[] GetKeys();
    }

    public class MultiRegionalDbContext<T> : IDisposable, IAsyncDisposable where T : BaseDbContext, new()
    {
        public MultiRegionalDbContext() { }

        internal List<T> Context { get; set; }

        public void Dispose()
        {
            if (Context == null) return;

            foreach (var c in Context)
            {
                if (c != null)
                {
                    c.Dispose();
                }
            }
        }
        public async ValueTask DisposeAsync()
        {
            if (Context == null) return;

            foreach (var c in Context)
            {
                if (c != null)
                {
                    await c.DisposeAsync();
                }
            }
        }
    }
}
