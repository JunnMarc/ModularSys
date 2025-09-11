using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModularSys.Data.Common.Entities.Inventory;

namespace ModularSys.Data.Common.Config
{
    public class PurchaseOrderLineConfig : IEntityTypeConfiguration<PurchaseOrderLine>
    {
        public void Configure(EntityTypeBuilder<PurchaseOrderLine> builder)
        {
            builder.ToTable("PurchaseOrderLines");

            builder.HasKey(pol => pol.PurchaseOrderLineId);

            builder.Property(pol => pol.UnitCost)
                   .HasColumnType("decimal(18,2)");

            builder.HasOne(pol => pol.Product)
                   .WithMany()
                   .HasForeignKey(pol => pol.ProductId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
