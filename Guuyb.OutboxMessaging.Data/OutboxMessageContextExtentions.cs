#if NET6_0 || NETCOREAPP3_1
using Microsoft.EntityFrameworkCore;
#endif

#if NET48
using System.Data.Entity;
# endif

using Guuyb.OutboxMessaging.Data.Models;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Diagnostics;

namespace Guuyb.OutboxMessaging.Data
{
    public static class OutboxMessageContextExtentions
    {
        /// <summary>
        /// Направить сообщение в определенную очередь
        /// </summary>
        public static void Send<TOutboxMessage>(this DbSet<TOutboxMessage> outboxMessages,
            object payload,
            string targetQueueName,
            Action<Optionals> setup = null)
            where TOutboxMessage : class, IOutboxMessage, new()
        {
            if (string.IsNullOrWhiteSpace(targetQueueName))
            {
                throw new ArgumentException(nameof(targetQueueName), "Sending assumes providing targetQueueName");
            }

            AddOutboxMessage(outboxMessages, payload, targetQueueName, setup);
        }

        /// <summary>
        /// Опубликовать сообщение
        /// </summary>
        public static void Publish<TOutboxMessage>(this DbSet<TOutboxMessage> outboxMessages,
            object payload,
            Action<Optionals> setup = null)
            where TOutboxMessage : class, IOutboxMessage, new()
        {
            AddOutboxMessage(outboxMessages, payload, setup: setup);
        }

        private static void AddOutboxMessage<TOutboxMessage>(DbSet<TOutboxMessage> outboxMessages,
            object payload,
            string targetQueueName = null,
            Action<Optionals> setup = null)
            where TOutboxMessage : class, IOutboxMessage, new()
        {

            if (outboxMessages is null)
            {
                throw new ArgumentNullException(nameof(outboxMessages));
            }

            if (payload is null)
            {
                throw new ArgumentNullException(nameof(payload));
            }

            var serializedPayload = JsonConvert.SerializeObject(payload);

            var optionals = new Optionals();
            setup?.Invoke(optionals);

            var adjustedEncoding = optionals.Encoding ?? Encoding.UTF8;

            outboxMessages.Add(new TOutboxMessage
            {
                CreateDate = DateTime.UtcNow,
                Payload = adjustedEncoding.GetBytes(serializedPayload),
                StringifiedPayload = serializedPayload,
                PayloadTypeName = optionals.SpecificTypeName ?? payload.GetType().Name,
                StateId = OutboxMessageStateEnum.New,
                TargetQueueName = targetQueueName,
                PublishAttemptCount = 0,
                RoutingKey = optionals.RoutingKey,
#if NET6_0 || NETCOREAPP3_1
                ParentActivityId = Activity.Current?.Id
#endif
            });
        }

        public class Optionals
        {
            public string RoutingKey { get; set; }
            public string SpecificTypeName { get; set; }
            public Encoding Encoding { get; set; }
        }
    }
}
