using Microsoft.EntityFrameworkCore;
using ModularSys.Data.Common.Db;
using ModularSys.Data.Common.Entities;

public class RolePermissionService : IRolePermissionService
{
    private readonly IDbContextFactory<ModularSysDbContext> _contextFactory;

    public RolePermissionService(IDbContextFactory<ModularSysDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<RolePermission>> GetAllAsync()
    {
        await using var db = _contextFactory.CreateDbContext();
        return await db.RolePermissions
            .Include(rp => rp.Role)
            .Include(rp => rp.Permission)
            .ToListAsync();
    }

    public async Task<RolePermission?> GetAsync(int roleId, int permissionId)
    {
        await using var db = _contextFactory.CreateDbContext();
        return await db.RolePermissions
            .Include(rp => rp.Role)
            .Include(rp => rp.Permission)
            .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);
    }

    public async Task<RolePermission> CreateAsync(RolePermission rolePermission)
    {
        await using var db = _contextFactory.CreateDbContext();
        db.RolePermissions.Add(rolePermission);
        await db.SaveChangesAsync();
        return rolePermission;
    }

    public async Task<bool> DeleteAsync(int roleId, int permissionId)
    {
        await using var db = _contextFactory.CreateDbContext();
        var rp = await db.RolePermissions.FindAsync(roleId, permissionId);
        if (rp == null) return false;

        db.RolePermissions.Remove(rp);
        return await db.SaveChangesAsync() > 0;
    }

    // 🔄 Toggle permission assignment
    public async Task<bool> ToggleAsync(int roleId, int permissionId, bool isGranted)
    {
        var existing = await GetAsync(roleId, permissionId);

        if (isGranted && existing is null)
        {
            await CreateAsync(new RolePermission { RoleId = roleId, PermissionId = permissionId });
            return true;
        }
        else if (!isGranted && existing is not null)
        {
            return await DeleteAsync(roleId, permissionId);
        }

        return false;
    }

    public async Task AssignPermissionsAsync(int roleId, List<int> permissionIds)
    {
        await using var db = _contextFactory.CreateDbContext();
        var existing = await db.RolePermissions
            .Where(rp => rp.RoleId == roleId)
            .Select(rp => rp.PermissionId)
            .ToListAsync();

        var toAdd = permissionIds.Except(existing);
        foreach (var pid in toAdd)
        {
            db.RolePermissions.Add(new RolePermission { RoleId = roleId, PermissionId = pid });
        }

        await db.SaveChangesAsync();
    }

    public async Task<List<Permission>> GetPermissionsForRoleAsync(int roleId)
    {
        await using var db = _contextFactory.CreateDbContext();
        return await db.RolePermissions
            .Where(rp => rp.RoleId == roleId)
            .Include(rp => rp.Permission)
            .Select(rp => rp.Permission)
            .Distinct()
            .ToListAsync();
    }

    public async Task<List<Permission>> GetPermissionsForRolesAsync(IEnumerable<int> roleIds)
    {
        await using var db = _contextFactory.CreateDbContext();
        return await db.RolePermissions
            .Where(rp => roleIds.Contains(rp.RoleId))
            .Include(rp => rp.Permission)
            .Select(rp => rp.Permission)
            .Distinct()
            .ToListAsync();
    }

}
