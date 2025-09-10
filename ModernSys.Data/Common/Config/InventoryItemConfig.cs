using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModularSys.Data.Common.Entities.Inventory;

namespace ModularSys.Data.Common.Config
{
    public class InventoryItemConfig : IEntityTypeConfiguration<InventoryItem>
    {
        public void Configure(EntityTypeBuilder<InventoryItem> builder)
        {
            builder.ToTable("InventoryItems");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(x => x.Price)
                   .HasColumnType("decimal(18,2)");
        }
    }
}

