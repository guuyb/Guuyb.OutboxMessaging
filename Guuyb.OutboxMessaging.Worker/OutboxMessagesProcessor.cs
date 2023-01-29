using EasyNetQ;
using EasyNetQ.Topology;
using EFCore.BulkExtensions;
using Guuyb.OutboxMessaging.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client.Exceptions;
using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Guuyb.OutboxMessaging.Worker
{
    internal class OutboxMessagesProcessor<TOutboxMessage>
        where TOutboxMessage : class, IOutboxMessage
    {
        private const int PUBLISH_ATTEMPT_COUNT_LIMIT = 3;

        private readonly ILogger<OutboxMessagesProcessor<TOutboxMessage>> _logger;
        private readonly IBus _bus;
        private readonly IOptions<OutboxMessagingWorkerConfig> _options;

        public OutboxMessagesProcessor(
            ILogger<OutboxMessagesProcessor<TOutboxMessage>> logger,
            IBus bus,
            IOptions<OutboxMessagingWorkerConfig> options)
        {
            _logger = logger;
            _bus = bus;
            _options = options;
        }

        public async Task ProcessAsync(DbSet<TOutboxMessage> _outboxMessageDbSet, CancellationToken stoppingToken)
        {
            if (_options.Value.IsNeedToDeletePublishedMessages)
            {
                await _outboxMessageDbSet
                    .Where(m => m.StateId == OutboxMessageStateEnum.Published)
                    .BatchDeleteAsync();
            }

            const int takeMessagesNumber = 100;
            var messages = await _outboxMessageDbSet
                .Where(m => m.StateId == OutboxMessageStateEnum.New)
                .OrderBy(m => m.CreateDate)
                .Take(takeMessagesNumber)
                .ToListAsync();

            if (!messages.Any())
                return;


            IExchange defaultExchange = null;
            if (!string.IsNullOrEmpty(_options.Value.DefaultExchangeName))
            {
                defaultExchange = await DeclareExchangeAsync(_options.Value.DefaultExchangeName, stoppingToken);
            }


            foreach (var message in messages)
            {
                try
                {
                    var properties = new MessageProperties();
                    properties.DeliveryMode = MessageDeliveryMode.Persistent;
                    properties.Type = message.PayloadTypeName;
                    properties.ContentType = "application/json";
                    properties.Headers.Add("CreateDate", message.CreateDate.ToString("O", CultureInfo.InvariantCulture));
                    properties.Headers.Add("ParentActivityId", message.ParentActivityId);

                    if (message.TargetQueueName != null)
                    {
                        if (_options.Value.IsNeedToDeclare)
                        {
                            await _bus.Advanced.QueueDeclareAsync(message.TargetQueueName, stoppingToken);
                        }
                        else
                        {
                            await _bus.Advanced.QueueDeclarePassiveAsync(message.TargetQueueName, stoppingToken);
                        }

                        // like _bus.SendReceive.SendAsync
                        await _bus.Advanced.PublishAsync(
                            Exchange.GetDefault(),
                            message.TargetQueueName,
                            mandatory: true,
                            properties,
                            message.Payload);
                    }
                    else
                    {
                        IExchange exchange;
                        if (defaultExchange != null)
                        {
                            exchange = defaultExchange;
                        }
                        else
                        {
                            exchange = await DeclareExchangeAsync(message.PayloadTypeName, stoppingToken);
                        }

                        // like _bus.PubSub.PublishAsync
                        await _bus.Advanced.PublishAsync(
                            exchange,
                            message.RoutingKey ?? string.Empty, // routing key
                            mandatory: true,
                            properties,
                            message.Payload,
                            stoppingToken);
                    }

                    message.StateId = OutboxMessageStateEnum.Published;
                    message.PublishDate = DateTime.UtcNow;
                }
                catch (BrokerUnreachableException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    // Если нет подключения к rabbit'у, то возвращается TaskCanceledException.
                    // Поэтому проверяем именно наличие подключения.
                    if (!_bus.Advanced.IsConnected)
                        throw;

                    message.PublishAttemptCount += 1;
                    if (message.PublishAttemptCount >= PUBLISH_ATTEMPT_COUNT_LIMIT)
                    {
                        message.StateId = OutboxMessageStateEnum.Error;
                    }

                    _logger.LogError(ex, "Can't publish message with id: {Id}", message.Id);
                }
            }
        }

        private async Task<IExchange> DeclareExchangeAsync(string exchangeName, CancellationToken stoppingToken)
        {
            IExchange exchange;
            if (_options.Value.IsNeedToDeclare)
            {
                exchange = await _bus.Advanced.ExchangeDeclareAsync(exchangeName,
                    configuration => configuration.WithType(ExchangeType.Topic), stoppingToken);
            }
            else
            {
                await _bus.Advanced.ExchangeDeclarePassiveAsync(exchangeName, stoppingToken);
                exchange = new Exchange(exchangeName, ExchangeType.Topic);
            }

            return exchange;
        }
    }
}
