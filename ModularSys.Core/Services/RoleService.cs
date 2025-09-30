using Microsoft.EntityFrameworkCore;
using ModularSys.Data.Common.Db;
using ModularSys.Data.Common.Entities;

public class RoleService : IRoleService
{
    private readonly IDbContextFactory<ModularSysDbContext> _contextFactory;

    public RoleService(IDbContextFactory<ModularSysDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<Role>> GetAllAsync()
    {
        await using var db = _contextFactory.CreateDbContext();
        return await db.Roles.Include(r => r.RolePermissions).ToListAsync();
    }

    public async Task<Role?> GetByIdAsync(int id)
    {
        await using var db = _contextFactory.CreateDbContext();
        return await db.Roles.Include(r => r.RolePermissions).FirstOrDefaultAsync(r => r.RoleId == id);
    }

    public async Task<Role> CreateAsync(Role role)
    {
        await using var db = _contextFactory.CreateDbContext();
        db.Roles.Add(role);
        await db.SaveChangesAsync();
        return role;
    }

    public async Task<bool> UpdateAsync(Role role)
    {
        await using var db = _contextFactory.CreateDbContext();
        db.Roles.Update(role);
        return await db.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        await using var db = _contextFactory.CreateDbContext();
        var role = await db.Roles.FindAsync(id);
        if (role == null) return false;
        db.Roles.Remove(role);
        return await db.SaveChangesAsync() > 0;
    }
}
