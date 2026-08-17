using Ambev.DeveloperEvaluation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ambev.DeveloperEvaluation.ORM.Mapping;

public class OutboxEventConfiguration : BaseEntityMapper<OutboxEvent>
{
    public override void Configure(EntityTypeBuilder<OutboxEvent> builder)
    {
        base.Configure(builder);

        builder.ToTable("OutboxEvents");

        builder.Property(x => x.EntityType)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.AggregateId)
            .IsRequired();

        builder.Property(x => x.EventType)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.Payload)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<int>();

        builder.Property(x => x.RetryCount)
            .IsRequired();

        builder.Property(x => x.LastError)
            .HasMaxLength(2000);

        builder.HasIndex(x => new { x.Status, x.NextAttemptAt });
    }
}
