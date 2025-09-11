using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModularSys.Data.Common.Entities.Inventory;

namespace ModularSys.Data.Common.Config
{
    public class ProductConfig : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.ToTable("Products");

            builder.HasKey(p => p.ProductId);

            builder.Property(p => p.Name)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(p => p.SKU)
                   .HasMaxLength(50);

            builder.Property(p => p.UnitPrice)
                   .HasColumnType("decimal(18,2)");

            builder.Property(p => p.CostPrice)
                   .HasColumnType("decimal(18,2)");

            builder.Property(p => p.QuantityOnHand)
                   .HasDefaultValue(0);

            builder.Property(p => p.ReorderLevel)
                   .HasDefaultValue(0);

            builder.HasIndex(p => p.Name);
            builder.HasIndex(p => p.SKU).IsUnique(false);

            builder.HasOne(p => p.Category)
                   .WithMany(c => c.Products)
                   .HasForeignKey(p => p.CategoryId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
