using Ambev.DeveloperEvaluation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ambev.DeveloperEvaluation.ORM.Mapping;

public class SaleConfiguration : BaseEntityMapper<Sale>
{
    public override void Configure(EntityTypeBuilder<Sale> builder)
    {
        base.Configure(builder);

        builder.ToTable("Sales");

        builder.Property(x => x.CartId)
            .IsRequired();

        builder.HasIndex(x => x.CartId)
            .IsUnique();

        builder.Property(x => x.OrderId)
            .IsRequired()
            .HasDefaultValueSql("nextval('\"SaleOrderNumbers\"')")
            .ValueGeneratedOnAdd();

        builder.HasIndex(x => x.OrderId)
            .IsUnique();

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.BranchId)
            .IsRequired();

        builder.Property(x => x.CustomerName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.CustomerEmail)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.BranchName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.BranchDocNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.BranchCompanyName)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.TotalAmount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.ProductsQuantity)
            .IsRequired();

        builder.Property(x => x.ItemsQuantity)
            .IsRequired();

        builder.Property(x => x.TotalDiscount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<int>();

        builder.OwnsOne(x => x.CustomerAddress, sa =>
        {
            sa.Property(a => a.City).HasColumnName("CustomerCity").HasMaxLength(100).IsRequired();
            sa.Property(a => a.Street).HasColumnName("CustomerStreet").HasMaxLength(200).IsRequired();
            sa.Property(a => a.Number).HasColumnName("CustomerNumber").IsRequired();
            sa.Property(a => a.PostalCode).HasColumnName("CustomerPostalCode").HasMaxLength(20).IsRequired();
            sa.Property(a => a.Latitude).HasColumnName("CustomerLatitude").HasMaxLength(50);
            sa.Property(a => a.Longitude).HasColumnName("CustomerLongitude").HasMaxLength(50);
        });

        builder.OwnsOne(x => x.BranchAddress, sa =>
        {
            sa.Property(a => a.City).HasColumnName("BranchCity").HasMaxLength(100).IsRequired();
            sa.Property(a => a.Street).HasColumnName("BranchStreet").HasMaxLength(200).IsRequired();
            sa.Property(a => a.Number).HasColumnName("BranchNumber").IsRequired();
            sa.Property(a => a.PostalCode).HasColumnName("BranchPostalCode").HasMaxLength(20).IsRequired();
            sa.Property(a => a.Latitude).HasColumnName("BranchLatitude").HasMaxLength(50);
            sa.Property(a => a.Longitude).HasColumnName("BranchLongitude").HasMaxLength(50);
        });

        builder.HasOne(x => x.Cart)
            .WithOne()
            .HasForeignKey<Sale>(x => x.CartId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Branch)
            .WithMany()
            .HasForeignKey(x => x.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Items)
            .WithOne(x => x.Sale)
            .HasForeignKey(x => x.SaleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
