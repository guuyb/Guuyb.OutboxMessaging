using Microsoft.EntityFrameworkCore;
using Guuyb.OutboxMessaging.Data.Models;
using System;

namespace Guuyb.OutboxMessaging.Data.Migrations.PostgreSql
{
    internal class OutboxMessageContext : DbContext
    {
        public OutboxMessageContext(DbContextOptions<OutboxMessageContext> options)
            : base(options)
        {
        }
        
        public DbSet<AnotherOutboxMessage> AnotherOutboxMessages { get; set; }
        public DbSet<AnotherOutboxMessageState> AnotherOutboxMessageStates { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // preferable way to register OutboxMessaging
            modelBuilder.ApplyDbSetOutboxMessagingConfiguration<AnotherOutboxMessage, AnotherOutboxMessageState>(
                nameof(AnotherOutboxMessage),
                nameof(AnotherOutboxMessageState));
        }
    }

    internal class SomeModel
    { }

    internal class AnotherOutboxMessage : IOutboxMessage
    {
        public DateTime CreateDate { get; set; }
        public int Id { get; set; }
        public byte[] Payload { get; set; }
        public string PayloadTypeName { get; set; }
        public int PublishAttemptCount { get; set; }
        public DateTime? PublishDate { get; set; }
        public OutboxMessageStateEnum StateId { get; set; }
        public string StringifiedPayload { get; set; }
        public string TargetQueueName { get; set; }
        public string RoutingKey { get; set; }
        public string ParentActivityId { get; set; }
    }

    internal class AnotherOutboxMessageState : IOutboxMessageState
    {
        public OutboxMessageStateEnum Id { get; set; }
        public string Code { get; set; }
    }
}
