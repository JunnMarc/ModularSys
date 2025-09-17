using Microsoft.EntityFrameworkCore;
using ModularSys.Data.Common.Entities.Finance;
using ModularSys.Data.Common.Entities.Inventory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModularSys.Data.Common.Interfaces;

namespace ModularSys.Data.Common.Db
{
    public class InventoryDbContext : DbContext
    {
        public InventoryDbContext(DbContextOptions<InventoryDbContext> options) : base(options) { }

        public DbSet<Product> Products => Set<Product>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<SalesOrder> SalesOrders => Set<SalesOrder>();
        public DbSet<SalesOrderLine> SalesOrderLines => Set<SalesOrderLine>();
        public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
        public DbSet<PurchaseOrderLine> PurchaseOrderLines => Set<PurchaseOrderLine>();
        public DbSet<InventoryTransaction> InventoryTransactions => Set<InventoryTransaction>();

        //For now
        public DbSet<RevenueTransaction> RevenueTransactions { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId);

            modelBuilder.Entity<SalesOrderLine>()
                .HasOne(l => l.Product)
                .WithMany()
                .HasForeignKey(l => l.ProductId);

            modelBuilder.Entity<PurchaseOrderLine>()
                .HasOne(l => l.Product)
                .WithMany()
                .HasForeignKey(l => l.ProductId);

            modelBuilder.Entity<InventoryTransaction>()
                .HasOne(t => t.Product)
                .WithMany()
                .HasForeignKey(t => t.ProductId);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(InventoryDbContext).Assembly);

            // Global query filters for soft delete
            modelBuilder.Entity<Product>().HasQueryFilter(p => !p.IsDeleted);
            modelBuilder.Entity<Category>().HasQueryFilter(c => !c.IsDeleted);
            modelBuilder.Entity<SalesOrder>().HasQueryFilter(s => !s.IsDeleted);
            modelBuilder.Entity<PurchaseOrder>().HasQueryFilter(p => !p.IsDeleted);
            modelBuilder.Entity<SalesOrderLine>().HasQueryFilter(sol => !sol.IsDeleted);
            modelBuilder.Entity<PurchaseOrderLine>().HasQueryFilter(pol => !pol.IsDeleted);
            modelBuilder.Entity<InventoryTransaction>().HasQueryFilter(t => !t.IsDeleted);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SetAuditProperties();
            return await base.SaveChangesAsync(cancellationToken);
        }

        public override int SaveChanges()
        {
            SetAuditProperties();
            return base.SaveChanges();
        }

        private void SetAuditProperties()
        {
            var now = DateTime.UtcNow;
            var currentUser = "System"; // TODO: replace with actual user context if available

            var softEntries = ChangeTracker.Entries()
                .Where(e => e.Entity is ISoftDeletable &&
                            (e.State == EntityState.Added ||
                             e.State == EntityState.Modified ||
                             e.State == EntityState.Deleted));

            foreach (var entry in softEntries)
            {
                var entity = (ISoftDeletable)entry.Entity;

                if (entry.State == EntityState.Added)
                {
                    entity.CreatedAt ??= now;
                    entity.CreatedBy ??= currentUser;
                    entity.IsDeleted = false;
                }

                if (entry.State == EntityState.Modified)
                {
                    entity.UpdatedAt = now;
                    entity.UpdatedBy = currentUser;
                }

                if (entry.State == EntityState.Deleted)
                {
                    entry.State = EntityState.Modified;
                    entity.IsDeleted = true;
                    entity.DeletedAt = now;
                    entity.DeletedBy = currentUser;
                    entity.UpdatedAt = now;
                    entity.UpdatedBy = currentUser;
                }
            }
        }

    }

}
