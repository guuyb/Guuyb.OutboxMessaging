#if NET6_0 || NETCOREAPP3_1
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Linq;
#endif

#if NET48
using System.Data.Entity.ModelConfiguration;
#endif

using Guuyb.OutboxMessaging.Data.Models;

namespace Guuyb.OutboxMessaging.Data.Configurations
{
    public partial class OutboxMessageStateConfiguration
    {
        public const int CODE_MAX_LENGTH = 50;

        private OutboxMessageStateConfiguration()
        { }
    }

#if NET6_0 || NETCOREAPP3_1
    public class OutboxMessageStateConfiguration<TOutboxMessageState> : IEntityTypeConfiguration<TOutboxMessageState>
        where TOutboxMessageState : class, IOutboxMessageState, new()
    {
        private readonly string _tableName;

        public OutboxMessageStateConfiguration(string tableName)
        {
            _tableName = tableName;
        }

        public void Configure(EntityTypeBuilder<TOutboxMessageState> builder)
        {
            builder.ToTable(_tableName);

            builder.HasKey(p => p.Id);

            builder
                .Property(p => p.Id)
                .HasConversion<int>();

            builder
                .Property(p => p.Code)
                .IsRequired()
                .HasMaxLength(OutboxMessageStateConfiguration.CODE_MAX_LENGTH);

            builder.HasData(
                Enum.GetValues(typeof(OutboxMessageStateEnum))
                    .Cast<OutboxMessageStateEnum>()
                    .Select(s => new TOutboxMessageState()
                    {
                        Id = s,
                        Code = s.ToString(),
                    }));
        }
    }
#endif

#if NET48
    public partial class OutboxMessageStateConfiguration<TOutboxMessageState> : EntityTypeConfiguration<TOutboxMessageState>
        where TOutboxMessageState : class, IOutboxMessageState
    {
        public OutboxMessageStateConfiguration(string tableName)
        {
            ToTable(tableName);

            HasKey(p => p.Id);

            Property(p => p.Code)
                .IsRequired()
                .HasMaxLength(OutboxMessageStateConfiguration.CODE_MAX_LENGTH);
        }
    }
#endif
}
