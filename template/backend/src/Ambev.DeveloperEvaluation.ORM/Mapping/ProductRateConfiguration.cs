using Ambev.DeveloperEvaluation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ambev.DeveloperEvaluation.ORM.Mapping;

public class ProductRateConfiguration : BaseEntityMapper<ProductRate>
{
    public override void Configure(EntityTypeBuilder<ProductRate> builder)
    {
        base.Configure(builder);

        builder.ToTable("ProductRates");

        builder.Property(x => x.ProductId)
            .IsRequired();

        builder.Property(x => x.AverageRate)
            .HasPrecision(5, 2)
            .IsRequired();

        builder.Property(x => x.ReviewCount)
            .IsRequired();

        builder.HasIndex(x => x.ProductId)
            .IsUnique()
            .HasFilter(SoftDeleteIndex.NotDeletedFilter);

        builder.HasOne(x => x.Product)
            .WithOne()
            .HasForeignKey<ProductRate>(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
