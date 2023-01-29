#if NET6_0 || NETCOREAPP3_1
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Linq.Expressions;
#endif

#if NET48
using System;
using System.Data.Entity.ModelConfiguration;
using System.Linq.Expressions;
#endif

using Guuyb.OutboxMessaging.Data.Models;

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

#if NET6_0 || NETCOREAPP3_1
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

            builder.Property(p => p.CreateDate)
                .HasConversion(
                    v => DateTime.SpecifyKind(v, DateTimeKind.Utc),
                    v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

            builder.Property(p => p.StateId)
                .HasConversion<int>();

            builder.Property(p => p.PublishDate)
                .HasConversion(
                    v => v != null ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : (DateTime?)null,
                    v => v != null ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : (DateTime?)null);

            builder.Property(p => p.Payload)
                .IsRequired();

            builder.Property(p => p.StringifiedPayload)
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
#endif

#if NET48
    /// <summary>
    /// Кофигурация для OutboxMessage (без связи с OutboxMessageState)
    /// </summary>
    /// <remarks>MsSql compatible</remarks>
    public class RelationlessOutboxMessageConfiguration<TOutboxMessage> : EntityTypeConfiguration<TOutboxMessage>
        where TOutboxMessage : class, IOutboxMessage
    {
        public RelationlessOutboxMessageConfiguration(string tableName)
        {
            ToTable(tableName);

            HasKey(p => p.Id);

            Property(p => p.CreateDate)
                .HasColumnType("datetime2")
                .HasPrecision(0);

            Property(p => p.PublishDate)
                .HasColumnType("datetime2")
                .HasPrecision(0);

            Property(p => p.Payload)
                .IsRequired()
                .HasColumnType("varbinary(max)");

            Property(p => p.StringifiedPayload)
                .IsRequired()
                .HasColumnType("nvarchar(max)");

            Property(p => p.PayloadTypeName)
                .IsRequired()
                .HasMaxLength(OutboxMessageConfiguration.PAYLOAD_TYPE_NAME_MAX_LENGTH);

            Property(p => p.TargetQueueName)
                .HasMaxLength(OutboxMessageConfiguration.TARGET_QUEUE_NAME_MAX_LENGTH);

            Property(p => p.RoutingKey)
                .HasMaxLength(OutboxMessageConfiguration.ROUTING_KEY_MAX_LENGTH);
        }
    }

    /// <summary>
    /// Кофигурация для OutboxMessage
    /// </summary>
    /// <remarks>Db independent</remarks>
    public class OutboxMessageConfiguration<TOutboxMessage, TOutboxMessageState> : EntityTypeConfiguration<TOutboxMessage>
        where TOutboxMessage : class, IOutboxMessage
        where TOutboxMessageState : class, IOutboxMessageState
    {
        public OutboxMessageConfiguration(string tableName,
            Expression<Func<TOutboxMessage, TOutboxMessageState>> stateNavigation)
        {
            if (stateNavigation is null)
            {
                throw new ArgumentNullException(nameof(stateNavigation));
            }

            ToTable(tableName);

            HasKey(p => p.Id);

            Property(p => p.CreateDate)
                .HasPrecision(0);

            Property(p => p.PublishDate)
                .HasPrecision(0);

            Property(p => p.StateId)
                .IsRequired();

            HasRequired(stateNavigation)
                .WithMany()
                .WillCascadeOnDelete(false);

            Property(p => p.Payload)
                .IsRequired();

            Property(p => p.StringifiedPayload)
                .IsRequired();

            Property(p => p.PayloadTypeName)
                .IsRequired()
                .HasMaxLength(OutboxMessageConfiguration.PAYLOAD_TYPE_NAME_MAX_LENGTH);

            Property(p => p.TargetQueueName)
                .HasMaxLength(OutboxMessageConfiguration.TARGET_QUEUE_NAME_MAX_LENGTH);

            Property(p => p.RoutingKey)
                .HasMaxLength(OutboxMessageConfiguration.ROUTING_KEY_MAX_LENGTH);
        }
    }
#endif
}
