using Ambev.DeveloperEvaluation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ambev.DeveloperEvaluation.ORM.Mapping;

public class BranchRateConfiguration : BaseEntityMapper<BranchRate>
{
    public override void Configure(EntityTypeBuilder<BranchRate> builder)
    {
        base.Configure(builder);

        builder.ToTable("BranchRates");

        builder.Property(x => x.BranchId)
            .IsRequired();

        builder.Property(x => x.AverageRate)
            .HasPrecision(5, 2)
            .IsRequired();

        builder.Property(x => x.ReviewCount)
            .IsRequired();

        builder.HasIndex(x => x.BranchId)
            .IsUnique();

        builder.HasOne(x => x.Branch)
            .WithOne()
            .HasForeignKey<BranchRate>(x => x.BranchId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
