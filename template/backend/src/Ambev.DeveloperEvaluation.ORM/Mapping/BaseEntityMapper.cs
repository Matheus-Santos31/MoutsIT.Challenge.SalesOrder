using Ambev.DeveloperEvaluation.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ambev.DeveloperEvaluation.ORM.Mapping;

/// <summary>
/// SQL filter for unique indexes on soft-deletable entities, so a deleted row doesn't keep
/// occupying the uniqueness slot forever — a new row with the same value can be added once
/// the old one is soft-deleted.
/// </summary>
public static class SoftDeleteIndex
{
    public const string NotDeletedFilter = "\"DeletedAt\" IS NULL";
}

public abstract class BaseEntityMapper<T> : IEntityTypeConfiguration<T> where T : BaseEntity
{
    public virtual void Configure(EntityTypeBuilder<T> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnType("uuid")
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(x => x.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired(false);

        builder.Property(x => x.DeletedAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired(false);

        builder.HasQueryFilter(x => x.DeletedAt == null);
    }
}
