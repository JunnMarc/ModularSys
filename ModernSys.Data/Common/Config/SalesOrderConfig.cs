using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModularSys.Data.Common.Entities.Inventory;

namespace ModularSys.Data.Common.Config
{
    public class SalesOrderConfig : IEntityTypeConfiguration<SalesOrder>
    {
        public void Configure(EntityTypeBuilder<SalesOrder> builder)
        {
            builder.ToTable("SalesOrders");

            builder.HasKey(so => so.SalesOrderId);

            builder.Property(so => so.OrderDate)
                   .IsRequired();

            builder.Property(so => so.TotalAmount)
                   .HasColumnType("decimal(18,2)");

            // Relationship: SalesOrder → SalesOrderLines
            builder.HasMany(so => so.Lines)
                   .WithOne(line => line.SalesOrder)
                   .HasForeignKey(line => line.SalesOrderId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
