using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModularSys.Data.Common.Entities.Inventory;

namespace ModularSys.Data.Common.Config
{
    public class InventoryTransactionConfig : IEntityTypeConfiguration<InventoryTransaction>
    {
        public void Configure(EntityTypeBuilder<InventoryTransaction> builder)
        {
            builder.ToTable("InventoryTransactions");

            builder.HasKey(t => t.InventoryTransactionId);

            builder.Property(t => t.TransactionType)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(t => t.Amount)
                   .HasColumnType("decimal(18,2)");

            builder.HasOne(t => t.Product)
                   .WithMany()
                   .HasForeignKey(t => t.ProductId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
