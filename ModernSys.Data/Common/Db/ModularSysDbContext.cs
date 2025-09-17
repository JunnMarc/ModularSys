using Microsoft.EntityFrameworkCore;
using ModularSys.Data.Common.Entities;
using ModularSys.Data.Common.Entities.Inventory;
using ModularSys.Data.Common.Entities.Finance;
using System;
using ModularSys.Data.Common.Interfaces;

namespace ModularSys.Data.Common.Db
{
    public class ModularSysDbContext : DbContext
    {
        // User Management
        public DbSet<User> Users => Set<User>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<Department> Departments => Set<Department>();
        public DbSet<Permission> Permissions => Set<Permission>();
        public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
        
        // Inventory Management
        public DbSet<Product> Products => Set<Product>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<InventoryTransaction> InventoryTransactions => Set<InventoryTransaction>();
        public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
        public DbSet<PurchaseOrderLine> PurchaseOrderLines => Set<PurchaseOrderLine>();
        public DbSet<SalesOrder> SalesOrders => Set<SalesOrder>();
        public DbSet<SalesOrderLine> SalesOrderLines => Set<SalesOrderLine>();
        
        // Finance
        public DbSet<RevenueTransaction> RevenueTransactions => Set<RevenueTransaction>();

        public ModularSysDbContext(DbContextOptions<ModularSysDbContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Composite key for RolePermission
            modelBuilder.Entity<RolePermission>()
                .HasKey(rp => new { rp.RoleId, rp.PermissionId });

            // Static seed data
            modelBuilder.Entity<Role>().HasData(
                new Role { RoleId = 1, RoleName = "Admin" }
            );

            modelBuilder.Entity<Department>().HasData(
                new Department { DepartmentId = 1, DepartmentName = "Administration", DepartmentDesc = "Handles system administration and configuration." }
            );

            modelBuilder.Entity<Permission>().HasData(
                new Permission { PermissionId = 1, PermissionName = "ManageUsers", Description = "Can manage users" },
                new Permission { PermissionId = 2, PermissionName = "ManageInventory", Description = "Can manage inventory" },
                new Permission { PermissionId = 3, PermissionName = "ViewReports", Description = "Can view reports" },
                new Permission { PermissionId = 4, PermissionName = "ManageDepartments", Description = "Can manage departments" }
            );

            modelBuilder.Entity<RolePermission>().HasData(
                new RolePermission { RoleId = 1, PermissionId = 1 },
                new RolePermission { RoleId = 1, PermissionId = 2 },
                new RolePermission { RoleId = 1, PermissionId = 3 },
                new RolePermission { RoleId = 1, PermissionId = 4 }
            );

            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Username = "admin",
                    FirstName = "System",
                    LastName = "Administrator",
                    PasswordHash = "JAvlGPq9JyTdtvBO6x2llnRI1+gxwIyPqCKAn3THIKk=", //admin123
                    Email = "admin@techvault.com",
                    ContactNumber = "+1-555-0100",
                    RoleId = 1,
                    DepartmentId = 1,
                    CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ModularSysDbContext).Assembly);
            
            // Apply soft delete query filters manually
            modelBuilder.Entity<User>().HasQueryFilter(u => !u.IsDeleted);
            
            // Inventory soft delete filters
            modelBuilder.Entity<Product>().HasQueryFilter(p => !p.IsDeleted);
            modelBuilder.Entity<Category>().HasQueryFilter(c => !c.IsDeleted);
            modelBuilder.Entity<SalesOrder>().HasQueryFilter(so => !so.IsDeleted);
            modelBuilder.Entity<PurchaseOrder>().HasQueryFilter(po => !po.IsDeleted);
            modelBuilder.Entity<SalesOrderLine>().HasQueryFilter(sol => !sol.IsDeleted);
            modelBuilder.Entity<PurchaseOrderLine>().HasQueryFilter(pol => !pol.IsDeleted);
            modelBuilder.Entity<InventoryTransaction>().HasQueryFilter(it => !it.IsDeleted);
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            SetAuditProperties(); // Set audit properties before saving
            return await base.SaveChangesAsync(cancellationToken);
        }

        public override int SaveChanges()
        {
            SetAuditProperties(); // Set audit properties before saving
            return base.SaveChanges();
        }

        private void SetAuditProperties()
        {
            // Note: Replace with real user context when available
            var currentUser = "System";
            var now = DateTime.UtcNow;

            // 1) Handle soft delete + audit for all ISoftDeletable entities (Inventory, etc.)
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

                // Convert hard delete into soft delete
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

            // 2) Preserve existing audit for User entity
            var userEntries = ChangeTracker.Entries<User>()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entry in userEntries)
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = now;
                    // entry.Entity.CreatedBy = currentUser; // hook up once user context is available
                }
                else if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedAt = now;
                    // entry.Entity.UpdatedBy = currentUser; // hook up once user context is available
                }
            }
        }
    }
}
