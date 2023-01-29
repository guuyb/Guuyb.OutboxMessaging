#if NET6_0 || NETCOREAPP3_1
using Microsoft.EntityFrameworkCore;
#endif

#if NET48
using System.Data.Entity;
#endif

using System;
using System.Linq.Expressions;
using Guuyb.OutboxMessaging.Data.Models;
using Guuyb.OutboxMessaging.Data.Configurations;

namespace Guuyb.OutboxMessaging.Data
{
    // todo: rename ModelBuilderExtensions
    public static class ModelBuilderExtentions
    {
#if NET6_0 || NETCOREAPP3_1
        public static ModelBuilder ApplyDbSetOutboxMessagingConfiguration<TOutboxMessage, TOutboxMessageState>(
            this ModelBuilder modelBuilder,
            string outboxMessageTableName,
            string outboxMessageStateTableName,
            Expression<Func<TOutboxMessage, TOutboxMessageState>> stateNavigation = null)
            where TOutboxMessage : class, IOutboxMessage
            where TOutboxMessageState : class, IOutboxMessageState, new()
        {
            if (modelBuilder is null)
            {
                throw new ArgumentNullException(nameof(modelBuilder));
            }

            return modelBuilder
                .ApplyConfiguration(
                    new Configurations.RelationfulOutboxMessageConfiguration<TOutboxMessage, TOutboxMessageState>(
                        outboxMessageTableName,
                        stateNavigation))
                .ApplyConfiguration(
                    new Configurations.OutboxMessageStateConfiguration<TOutboxMessageState>(outboxMessageStateTableName));
        }
    }
#endif

#if NET48
        public static DbModelBuilder ApplyDbSetOutboxMessagingConfiguration<TOutboxMessage, TOutboxMessageState>(
            this DbModelBuilder modelBuilder,
            string outboxMessageTableName,
            string outboxMessageStateTableName,
            Expression<Func<TOutboxMessage, TOutboxMessageState>> stateNavigation = null)
            where TOutboxMessage : class, IOutboxMessage
            where TOutboxMessageState : class, IOutboxMessageState, new()
        {
            if (modelBuilder is null)
            {
                throw new ArgumentNullException(nameof(modelBuilder));
            }

            modelBuilder.Configurations.Add(new OutboxMessageConfiguration<TOutboxMessage, TOutboxMessageState>(outboxMessageTableName, stateNavigation));
            modelBuilder.Configurations.Add(new OutboxMessageStateConfiguration<TOutboxMessageState>(outboxMessageStateTableName));

            return modelBuilder;
        }
    }
#endif
}
