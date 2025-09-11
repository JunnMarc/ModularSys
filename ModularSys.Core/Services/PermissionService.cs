using Microsoft.EntityFrameworkCore;
using ModularSys.Data.Common.Db;
using ModularSys.Data.Common.Entities;

public class PermissionService : IPermissionService
{
    private readonly IDbContextFactory<ModularSysDbContext> _contextFactory;

    public PermissionService(IDbContextFactory<ModularSysDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<Permission>> GetAllAsync()
    {
        await using var db = _contextFactory.CreateDbContext();
        return await db.Permissions
            .Include(p => p.RolePermissions)
            .ToListAsync();
    }

    public async Task<Permission?> GetByIdAsync(int id)
    {
        await using var db = _contextFactory.CreateDbContext();
        return await db.Permissions
            .Include(p => p.RolePermissions)
            .FirstOrDefaultAsync(p => p.PermissionId == id);
    }

    public async Task<Permission> CreateAsync(Permission permission)
    {
        await using var db = _contextFactory.CreateDbContext();
        db.Permissions.Add(permission);
        await db.SaveChangesAsync();
        return permission;
    }

    public async Task<bool> UpdateAsync(Permission permission)
    {
        await using var db = _contextFactory.CreateDbContext();
        db.Permissions.Update(permission);
        return await db.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        await using var db = _contextFactory.CreateDbContext();
        var permission = await db.Permissions.FindAsync(id);
        if (permission == null) return false;
        db.Permissions.Remove(permission);
        return await db.SaveChangesAsync() > 0;
    }

    public async Task<Permission?> GetByNameAsync(string permissionName)
    {
        await using var db = _contextFactory.CreateDbContext();
        return await db.Permissions
            .SingleOrDefaultAsync(p => p.PermissionName == permissionName);
    }
}
