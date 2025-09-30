using Microsoft.EntityFrameworkCore;
using ModularSys.Core.Interfaces;
using ModularSys.Data.Common.Db;
using ModularSys.Data.Common.Entities;

namespace ModularSys.Core.Services;

public class DepartmentService : IDepartmentService
{
    private readonly IDbContextFactory<ModularSysDbContext> _contextFactory;

    public DepartmentService(IDbContextFactory<ModularSysDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task<List<Department>> GetAllAsync()
    {
        await using var db = _contextFactory.CreateDbContext();
        return await db.Departments.ToListAsync();
    }

    public async Task<Department?> GetByIdAsync(int id)
    {
        await using var db = _contextFactory.CreateDbContext();
        return await db.Departments.FindAsync(id);
    }

    public async Task<Department> CreateAsync(Department department)
    {
        await using var db = _contextFactory.CreateDbContext();
        db.Departments.Add(department);
        await db.SaveChangesAsync();
        return department;
    }

    public async Task<bool> UpdateAsync(Department department)
    {
        await using var db = _contextFactory.CreateDbContext();
        db.Departments.Update(department);
        return await db.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        await using var db = _contextFactory.CreateDbContext();
        var department = await db.Departments.FindAsync(id);
        if (department == null) return false;
        db.Departments.Remove(department);
        return await db.SaveChangesAsync() > 0;
    }
}
