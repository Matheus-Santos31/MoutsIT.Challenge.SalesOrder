using Ambev.DeveloperEvaluation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ambev.DeveloperEvaluation.ORM.Mapping;

public class BranchEvaluationConfiguration : BaseEntityMapper<BranchEvaluation>
{
    public override void Configure(EntityTypeBuilder<BranchEvaluation> builder)
    {
        base.Configure(builder);

        builder.ToTable("BranchEvaluations");

        builder.Property(x => x.BranchId)
            .IsRequired();

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.Rate)
            .HasPrecision(5, 2)
            .IsRequired();

        builder.Property(x => x.Comment)
            .HasMaxLength(1000);

        builder.HasIndex(x => new { x.BranchId, x.UserId })
            .IsUnique()
            .HasFilter(SoftDeleteIndex.NotDeletedFilter);

        builder.HasOne(x => x.Branch)
            .WithMany(x => x.Evaluations)
            .HasForeignKey(x => x.BranchId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
