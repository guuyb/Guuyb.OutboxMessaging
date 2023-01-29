using Guuyb.OutboxMessaging.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Guuyb.OutboxMessaging.Worker
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDbSetOutboxMessagingWorker<TOutboxMessage, TDBContext>(this IServiceCollection services,
            Action<OutboxMessagingWorkerConfig> setup = null)
            where TDBContext : DbContext
            where TOutboxMessage : class, IOutboxMessage
        {
            if (setup != null)
                services.Configure(setup);
            services.AddHostedService<DbSetOutboxMessagingWorker<TOutboxMessage, TDBContext>>();
            services.AddTransient<OutboxMessagesProcessor<TOutboxMessage>>();

            return services;
        }
    }
}
