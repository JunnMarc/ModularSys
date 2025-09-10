using Microsoft.EntityFrameworkCore;
using ModularSys.Data.Common.Db;
using ModularSys.Data.Common.Entities;

public class RolePermissionService : IRolePermissionService
{
    private readonly ModularSysDbContext _db;

    public RolePermissionService(ModularSysDbContext db)
    {
        _db = db;
    }

    public async Task<List<RolePermission>> GetAllAsync() =>
        await _db.RolePermissions
            .Include(rp => rp.Role)
            .Include(rp => rp.Permission)
            .ToListAsync();

    public async Task<RolePermission?> GetAsync(int roleId, int permissionId) =>
        await _db.RolePermissions
            .Include(rp => rp.Role)
            .Include(rp => rp.Permission)
            .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);

    public async Task<RolePermission> CreateAsync(RolePermission rolePermission)
    {
        _db.RolePermissions.Add(rolePermission);
        await _db.SaveChangesAsync();
        return rolePermission;
    }

    public async Task<bool> DeleteAsync(int roleId, int permissionId)
    {
        var rp = await _db.RolePermissions.FindAsync(roleId, permissionId);
        if (rp == null) return false;

        _db.RolePermissions.Remove(rp);
        return await _db.SaveChangesAsync() > 0;
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
        var existing = await _db.RolePermissions
            .Where(rp => rp.RoleId == roleId)
            .Select(rp => rp.PermissionId)
            .ToListAsync();

        var toAdd = permissionIds.Except(existing);
        foreach (var pid in toAdd)
        {
            _db.RolePermissions.Add(new RolePermission { RoleId = roleId, PermissionId = pid });
        }

        await _db.SaveChangesAsync();
    }

    public async Task<List<Permission>> GetPermissionsForRoleAsync(int roleId)
    {
        return await _db.RolePermissions
            .Where(rp => rp.RoleId == roleId)
            .Include(rp => rp.Permission)
            .Select(rp => rp.Permission)
            .ToListAsync();
    }
}
