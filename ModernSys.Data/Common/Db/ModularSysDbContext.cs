using Microsoft.EntityFrameworkCore;
using ModularSys.Data.Common.Entities;
using ModularSys.Data.Common.Entities.Inventory;
using System;

namespace ModularSys.Data.Common.Db
{
    public class ModularSysDbContext : DbContext
    {
        public DbSet<User> Users => Set<User>();
        public DbSet<Product> InventoryItems => Set<Product>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<Department> Departments => Set<Department>();
        public DbSet<Permission> Permissions => Set<Permission>();
        public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

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
                    PasswordHash = "JAvlGPq9JyTdtvBO6x2llnRI1+gxwIyPqCKAn3THIKk=", //admin123
                    Email = "admin@techvault.com",
                    RoleId = 1,
                    DepartmentId = 1,
                    CreatedAt = new DateTime(2024, 01, 01, 0, 0, 0, DateTimeKind.Utc)
                }
            );

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ModularSysDbContext).Assembly);
        }
    }
}
