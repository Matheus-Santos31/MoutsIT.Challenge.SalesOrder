using Ambev.DeveloperEvaluation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ambev.DeveloperEvaluation.ORM.Mapping;

public class BranchManagerConfiguration : BaseEntityMapper<BranchManager>
{
    public override void Configure(EntityTypeBuilder<BranchManager> builder)
    {
        base.Configure(builder);

        builder.ToTable("BranchManagers");

        builder.Property(x => x.BranchId).IsRequired();
        builder.Property(x => x.UserId).IsRequired();

        builder.HasIndex(x => x.UserId).IsUnique();

        builder.HasOne(x => x.Branch)
            .WithMany()
            .HasForeignKey(x => x.BranchId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
