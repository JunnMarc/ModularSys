using Microsoft.EntityFrameworkCore;
using ModularSys.Data.Common.Db;
using ModularSys.Data.Common.Entities;

public class PermissionService : IPermissionService
{
    private readonly ModularSysDbContext _db;

    public PermissionService(ModularSysDbContext db)
    {
        _db = db;
    }

    public async Task<List<Permission>> GetAllAsync() =>
        await _db.Permissions.Include(p => p.RolePermissions).ToListAsync();

    public async Task<Permission?> GetByIdAsync(int id) =>
        await _db.Permissions.Include(p => p.RolePermissions).FirstOrDefaultAsync(p => p.PermissionId == id);

    public async Task<Permission> CreateAsync(Permission permission)
    {
        _db.Permissions.Add(permission);
        await _db.SaveChangesAsync();
        return permission;
    }

    public async Task<bool> UpdateAsync(Permission permission)
    {
        _db.Permissions.Update(permission);
        return await _db.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var permission = await _db.Permissions.FindAsync(id);
        if (permission == null) return false;
        _db.Permissions.Remove(permission);
        return await _db.SaveChangesAsync() > 0;
    }
}
