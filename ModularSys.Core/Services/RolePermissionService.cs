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
        await _db.RolePermissions.Include(rp => rp.Role).Include(rp => rp.Permission).ToListAsync();

    public async Task<RolePermission?> GetAsync(int roleId, int permissionId) =>
        await _db.RolePermissions.Include(rp => rp.Role).Include(rp => rp.Permission)
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
}
