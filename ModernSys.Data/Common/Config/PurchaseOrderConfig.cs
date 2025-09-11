using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModularSys.Data.Common.Entities.Inventory;

namespace ModularSys.Data.Common.Config
{
    public class PurchaseOrderConfig : IEntityTypeConfiguration<PurchaseOrder>
    {
        public void Configure(EntityTypeBuilder<PurchaseOrder> builder)
        {
            builder.ToTable("PurchaseOrders");

            builder.HasKey(po => po.PurchaseOrderId);

            builder.Property(po => po.Status)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(po => po.TotalAmount)
                   .HasColumnType("decimal(18,2)");

            builder.HasMany(po => po.Lines)
                   .WithOne(l => l.PurchaseOrder)
                   .HasForeignKey(l => l.PurchaseOrderId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
