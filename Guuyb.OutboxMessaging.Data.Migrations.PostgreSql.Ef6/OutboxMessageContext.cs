using Guuyb.OutboxMessaging.Data.Models;
using System;
using System.Data.Entity;

namespace Guuyb.OutboxMessaging.Data.Migrations.MicrosoftSql
{
    internal class OutboxMessageContext : DbContext
    {
        public OutboxMessageContext() : base("name=OutboxMessageContext")
        {
        }

        public DbSet<AnotherOutboxMessage> AnotherOutboxMessages { get; set; }
        public DbSet<AnotherOutboxMessageState> AnotherOutboxMessageStates { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("public"); // defualt for PostgreSql

            modelBuilder.ApplyDbSetOutboxMessagingConfiguration<AnotherOutboxMessage, AnotherOutboxMessageState>(
                "AnotherOutboxMessage",
                "AnotherOutboxMessageState",
                m => m.State);
        }
    }

    internal class AnotherOutboxMessage : IOutboxMessage
    {
        public DateTime CreateDate { get; set; }
        public int Id { get; set; }
        public byte[] Payload { get; set; }
        public string PayloadTypeName { get; set; }
        public int PublishAttemptCount { get; set; }
        public DateTime? PublishDate { get; set; }
        public OutboxMessageStateEnum StateId { get; set; }
        public AnotherOutboxMessageState State { get; set; }
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
