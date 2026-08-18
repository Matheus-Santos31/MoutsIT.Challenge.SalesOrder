using Ambev.DeveloperEvaluation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ambev.DeveloperEvaluation.ORM.Mapping;

public class ProductEvaluationConfiguration : BaseEntityMapper<ProductEvaluation>
{
    public override void Configure(EntityTypeBuilder<ProductEvaluation> builder)
    {
        base.Configure(builder);

        builder.ToTable("ProductEvaluations");

        builder.Property(x => x.ProductId)
            .IsRequired();

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.Rate)
            .HasPrecision(5, 2)
            .IsRequired();

        builder.Property(x => x.Comment)
            .HasMaxLength(1000);

        builder.HasIndex(x => new { x.ProductId, x.UserId })
            .IsUnique()
            .HasFilter(SoftDeleteIndex.NotDeletedFilter);

        builder.HasOne(x => x.Product)
            .WithMany(x => x.Evaluations)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
