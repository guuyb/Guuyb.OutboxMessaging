using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Guuyb.OutboxMessaging.Data.Migrations.PostgreSql;
using System;
using System.IO;

namespace Guuyb.OutboxMessaging.Data.Core.Test
{
    public class PostgreSqlDesignTimeDbContxtFactory : IDesignTimeDbContextFactory<OutboxMessageContext>
    {
        OutboxMessageContext IDesignTimeDbContextFactory<OutboxMessageContext>.CreateDbContext(string[] args)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                 .SetBasePath(Directory.GetCurrentDirectory())
                 .AddJsonFile("appsettings.json")
                 .Build();

            var builder = new DbContextOptionsBuilder<OutboxMessageContext>();

            var connectionString = configuration.GetConnectionString("OutboxMessageContext");

            builder.UseNpgsql(connectionString, o =>
            {
                o.MigrationsAssembly(GetType().Assembly.FullName);
                o.SetPostgresVersion(new Version("9.6"));
            });

            return new OutboxMessageContext(builder.Options);
        }
    }
}
