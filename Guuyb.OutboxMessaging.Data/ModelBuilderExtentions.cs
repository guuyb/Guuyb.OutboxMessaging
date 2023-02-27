using Guuyb.OutboxMessaging.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq.Expressions;

namespace Guuyb.OutboxMessaging.Data
{
    public static class ModelBuilderExtentions
    {
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
}
