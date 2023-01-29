using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace Guuyb.OutboxMessaging.Data.Migrations.MicrosoftSql
{
    public class MsSqlDesignTimeDbContextFactory : IDesignTimeDbContextFactory<OutboxMessageContext>
    {
        OutboxMessageContext IDesignTimeDbContextFactory<OutboxMessageContext>.CreateDbContext(string[] args)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                 .SetBasePath(Directory.GetCurrentDirectory())
                 .AddJsonFile("appsettings.json")
                 .Build();

            var builder = new DbContextOptionsBuilder<OutboxMessageContext>();

            var connectionString = configuration.GetConnectionString("OutboxMessageContext");

            builder.UseSqlServer(connectionString);

            return new OutboxMessageContext(builder.Options);
        }
    }
}
