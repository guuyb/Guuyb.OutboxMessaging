using Guuyb.OutboxMessaging.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Guuyb.OutboxMessaging.Worker
{
    /// <summary>
    /// Перемещает сообщения из таблицы в шину сообщений
    /// </summary>
    internal class DbSetOutboxMessagingWorker<TOutboxMessage, TDBContext> : BackgroundService
        where TDBContext : DbContext
        where TOutboxMessage : class, IOutboxMessage
    {
        private const int DEFAULT_DELAY = 1000;

        private readonly ILogger<DbSetOutboxMessagingWorker<TOutboxMessage, TDBContext>> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly OutboxMessagingWorkerConfig _config;

        public DbSetOutboxMessagingWorker(
            ILogger<DbSetOutboxMessagingWorker<TOutboxMessage, TDBContext>> logger,
            IServiceProvider serviceProvider,
            IOptions<OutboxMessagingWorkerConfig> options)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _config = options.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(_config.DelayBetweenExecutions ?? DEFAULT_DELAY, stoppingToken);

                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var context = scope.ServiceProvider.GetService<TDBContext>();
                    var outboxMessagesProcessor = scope.ServiceProvider.GetService<OutboxMessagesProcessor<TOutboxMessage>>();
                    await outboxMessagesProcessor.ProcessAsync(context.Set<TOutboxMessage>(), stoppingToken);

                    await context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "New messages is not published");
                }
            }
        }
    }
}

