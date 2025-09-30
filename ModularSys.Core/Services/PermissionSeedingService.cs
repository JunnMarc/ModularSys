using Microsoft.EntityFrameworkCore;
using ModularSys.Core.Security;
using ModularSys.Data.Common.Db;
using ModularSys.Data.Common.Entities;

namespace ModularSys.Core.Services;

public interface IPermissionSeedingService
{
    Task SeedPermissionsAsync();
    Task SeedRoleTemplatesAsync();
}

public class PermissionSeedingService : IPermissionSeedingService
{
    private readonly IDbContextFactory<ModularSysDbContext> _contextFactory;

    public PermissionSeedingService(IDbContextFactory<ModularSysDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task SeedPermissionsAsync()
    {
        await using var db = _contextFactory.CreateDbContext();
        var existingPermissions = await db.Permissions.Select(p => p.PermissionName).ToListAsync();

        var permissionsToAdd = new List<Permission>();
        var displayOrder = 1;

        foreach (var category in PermissionConstants.PermissionCategories)
        {
            foreach (var permissionName in category.Value)
            {
                if (!existingPermissions.Contains(permissionName))
                {
                    var permission = new Permission
                    {
                        PermissionName = permissionName,
                        Description = PermissionConstants.PermissionDescriptions.GetValueOrDefault(permissionName, $"Permission for {permissionName}"),
                        Category = category.Key,
                        Icon = GetPermissionIcon(permissionName),
                        DisplayOrder = displayOrder++,
                        IsSystemPermission = true,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = "System"
                    };
                    permissionsToAdd.Add(permission);
                }
            }
        }

        if (permissionsToAdd.Any())
        {
            db.Permissions.AddRange(permissionsToAdd);
            await db.SaveChangesAsync();
            Console.WriteLine($"Seeded {permissionsToAdd.Count} new permissions");
        }
    }

    public async Task SeedRoleTemplatesAsync()
    {
        await using var db = _contextFactory.CreateDbContext();
        var existingRoles = await db.Roles.Select(r => r.RoleName).ToListAsync();
        var allPermissions = await db.Permissions.ToListAsync();

        foreach (var template in PermissionConstants.RoleTemplates)
        {
            if (!existingRoles.Contains(template.Key))
            {
                // Create the role
                var role = new Role
                {
                    RoleName = template.Key,
                    Description = GetRoleDescription(template.Key),
                    CreatedAt = DateTime.UtcNow
                };

                db.Roles.Add(role);
                await db.SaveChangesAsync();

                // Assign permissions
                var rolePermissions = new List<RolePermission>();
                foreach (var permissionName in template.Value)
                {
                    var permission = allPermissions.FirstOrDefault(p => p.PermissionName == permissionName);
                    if (permission != null)
                    {
                        rolePermissions.Add(new RolePermission
                        {
                            RoleId = role.RoleId,
                            PermissionId = permission.PermissionId
                        });
                    }
                }

                if (rolePermissions.Any())
                {
                    db.RolePermissions.AddRange(rolePermissions);
                    await db.SaveChangesAsync();
                }

                Console.WriteLine($"Created role template: {template.Key} with {rolePermissions.Count} permissions");
            }
        }
    }

    private string GetPermissionIcon(string permissionName) => permissionName switch
    {
        var p when p.Contains("View") => "Visibility",
        var p when p.Contains("Create") => "Add",
        var p when p.Contains("Edit") => "Edit",
        var p when p.Contains("Delete") => "Delete",
        var p when p.Contains("Export") => "Download",
        var p when p.Contains("Import") => "Upload",
        var p when p.Contains("Manage") => "Settings",
        var p when p.Contains("Dashboard") => "Dashboard",
        var p when p.Contains("Reports") => "Assessment",
        var p when p.Contains("System") => "AdminPanelSettings",
        var p when p.Contains("Audit") => "History",
        var p when p.Contains("Backup") => "Backup",
        _ => "Lock"
    };

    private string GetRoleDescription(string roleName) => roleName switch
    {
        "Super Administrator" => "Full system access with all permissions",
        "System Administrator" => "Administrative access to most system functions",
        "HR Manager" => "Human resources management with user and department access",
        "Department Manager" => "Limited management access for department operations",
        "Regular User" => "Standard user access with basic functionality",
        "Read Only" => "View-only access to system information",
        _ => $"Role for {roleName}"
    };
}
