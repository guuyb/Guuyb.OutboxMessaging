using Guuyb.OutboxMessaging.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Linq.Expressions;

namespace Guuyb.OutboxMessaging.Data.Configurations
{
    internal static class OutboxMessageConfiguration
    {
        // оптимистичный вариант для RabbitMq, когда наименование состоит из латинских символов
        private const int OPTIMISTIC_QUEUE_NAME_MAX_LENGTH = 255;

        public const int PAYLOAD_TYPE_NAME_MAX_LENGTH = OPTIMISTIC_QUEUE_NAME_MAX_LENGTH;
        public const int TARGET_QUEUE_NAME_MAX_LENGTH = OPTIMISTIC_QUEUE_NAME_MAX_LENGTH;
        public const int ROUTING_KEY_MAX_LENGTH = OPTIMISTIC_QUEUE_NAME_MAX_LENGTH;
        public const int PARENT_ACTIVITY_ID_MAX_LENGTH = 2048;
    }

    public class RelationlessOutboxMessageConfiguration<TOutboxMessage> : IEntityTypeConfiguration<TOutboxMessage>
        where TOutboxMessage : class, IOutboxMessage
    {
        private readonly string _tableName;

        public RelationlessOutboxMessageConfiguration(string tableName)
        {
            _tableName = tableName;
        }

        public void Configure(EntityTypeBuilder<TOutboxMessage> builder)
        {
            builder.ToTable(_tableName);

            builder.HasKey(p => p.Id);

            builder.Property(p => p.CreatedAt)
                .HasConversion(
                    v => v.ToUniversalTime(),
                    v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

            builder.Property(p => p.StateId)
                .HasConversion<int>();

            builder.Property(p => p.PublishedAt)
                .HasConversion(
                    v => v != null ? v.Value.ToUniversalTime() : (DateTime?)null,
                    v => v != null ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : (DateTime?)null);

            builder.Property(p => p.Payload)
                .IsRequired();

            builder.Property(p => p.PayloadTypeName)
                .IsRequired()
                .HasMaxLength(OutboxMessageConfiguration.PAYLOAD_TYPE_NAME_MAX_LENGTH);

            builder.Property(p => p.TargetQueueName)
                .IsRequired(false)
                .HasMaxLength(OutboxMessageConfiguration.TARGET_QUEUE_NAME_MAX_LENGTH);

            builder.Property(p => p.RoutingKey)
                .IsRequired(false)
                .HasMaxLength(OutboxMessageConfiguration.ROUTING_KEY_MAX_LENGTH);

            builder.Property(p => p.ParentActivityId)
                .IsRequired(false)
                .HasMaxLength(OutboxMessageConfiguration.PARENT_ACTIVITY_ID_MAX_LENGTH);

            builder.Property(p => p.DelayUntil)
                .HasConversion(
                    v => v != null ? v.Value.ToUniversalTime() : (DateTime?)null,
                    v => v != null ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : null);
        }
    }

    public class RelationfulOutboxMessageConfiguration<TOutboxMessage, TOutboxMessageState> : IEntityTypeConfiguration<TOutboxMessage>
        where TOutboxMessage : class, IOutboxMessage
        where TOutboxMessageState : class, IOutboxMessageState
    {
        private readonly Expression<Func<TOutboxMessage, TOutboxMessageState>> _stateNavigation;
        private readonly RelationlessOutboxMessageConfiguration<TOutboxMessage> _relationlessOutboxMessageConfiguration;

        public RelationfulOutboxMessageConfiguration(string tableName,
            Expression<Func<TOutboxMessage, TOutboxMessageState>> stateNavigation = null)
        {
            _stateNavigation = stateNavigation;
            _relationlessOutboxMessageConfiguration = new RelationlessOutboxMessageConfiguration<TOutboxMessage>(tableName);
        }

        public void Configure(EntityTypeBuilder<TOutboxMessage> builder)
        {
            _relationlessOutboxMessageConfiguration.Configure(builder);

            builder
                .HasOne(_stateNavigation)
                .WithMany()
                .HasForeignKey(p => p.StateId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
