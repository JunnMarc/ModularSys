using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModularSys.Data.Common.Entities.Inventory;

namespace ModularSys.Data.Common.Config
{
    public class SalesOrderLineConfig : IEntityTypeConfiguration<SalesOrderLine>
    {
        public void Configure(EntityTypeBuilder<SalesOrderLine> builder)
        {
            builder.ToTable("SalesOrderLines");

            builder.HasKey(sol => sol.SalesOrderLineId);

            builder.Property(sol => sol.UnitPrice)
                   .HasColumnType("decimal(18,2)");

            builder.Property(sol => sol.Quantity)
                   .IsRequired();

            // Relationship: SalesOrderLine → Product
            builder.HasOne(sol => sol.Product)
                   .WithMany()
                   .HasForeignKey(sol => sol.ProductId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
