using Ambev.DeveloperEvaluation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ambev.DeveloperEvaluation.ORM.Mapping;

public class BranchAddressConfiguration : BaseEntityMapper<BranchAddress>
{
    public override void Configure(EntityTypeBuilder<BranchAddress> builder)
    {
        base.Configure(builder);

        builder.ToTable("BranchAddresses");

        builder.Property(x => x.BranchId)
            .IsRequired();

        builder.Property(x => x.AddressId)
            .IsRequired();

        builder.HasIndex(x => new { x.BranchId, x.AddressId })
            .IsUnique()
            .HasFilter(SoftDeleteIndex.NotDeletedFilter);

        builder.HasOne(x => x.Branch)
            .WithMany()
            .HasForeignKey(x => x.BranchId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Address)
            .WithMany()
            .HasForeignKey(x => x.AddressId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
