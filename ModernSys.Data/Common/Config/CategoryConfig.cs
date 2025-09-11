using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ModularSys.Data.Common.Entities.Inventory;

namespace ModularSys.Data.Common.Config
{
    public class CategoryConfig : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.ToTable("Categories");

            builder.HasKey(c => c.CategoryId);

            builder.Property(c => c.CategoryName)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(c => c.IsRevenueCritical)
                   .HasDefaultValue(false);

            builder.HasIndex(c => c.CategoryName).IsUnique();
        }
    }
}
