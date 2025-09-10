using Microsoft.EntityFrameworkCore;
using ModularSys.Data.Common.Db;
using ModularSys.Data.Common.Entities;

public class RoleService : IRoleService
{
    private readonly ModularSysDbContext _db;

    public RoleService(ModularSysDbContext db)
    {
        _db = db;
    }

    public async Task<List<Role>> GetAllAsync() =>
        await _db.Roles.Include(r => r.RolePermissions).ToListAsync();

    public async Task<Role?> GetByIdAsync(int id) =>
        await _db.Roles.Include(r => r.RolePermissions).FirstOrDefaultAsync(r => r.RoleId == id);

    public async Task<Role> CreateAsync(Role role)
    {
        _db.Roles.Add(role);
        await _db.SaveChangesAsync();
        return role;
    }

    public async Task<bool> UpdateAsync(Role role)
    {
        _db.Roles.Update(role);
        return await _db.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var role = await _db.Roles.FindAsync(id);
        if (role == null) return false;
        _db.Roles.Remove(role);
        return await _db.SaveChangesAsync() > 0;
    }
}
