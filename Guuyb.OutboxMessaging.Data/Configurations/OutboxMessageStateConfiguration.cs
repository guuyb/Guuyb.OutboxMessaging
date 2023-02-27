using Guuyb.OutboxMessaging.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Linq;

namespace Guuyb.OutboxMessaging.Data.Configurations
{
    public class OutboxMessageStateConfiguration<TOutboxMessageState> : IEntityTypeConfiguration<TOutboxMessageState>
        where TOutboxMessageState : class, IOutboxMessageState, new()
    {
        private readonly string _tableName;

        public const int CODE_MAX_LENGTH = 50;

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
                .HasMaxLength(CODE_MAX_LENGTH);

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
}
