using Microsoft.EntityFrameworkCore;
using ModularSys.Data.Common.Db;
using ModularSys.Data.Common.Entities;
using ModularSys.Core.Interfaces;

namespace ModularSys.Core.Services;

public class DepartmentService : IDepartmentService
{
    private readonly ModularSysDbContext _db;

    public DepartmentService(ModularSysDbContext db)
    {
        _db = db;
    }

    public async Task<List<Department>> GetAllAsync() =>
        await _db.Departments.OrderBy(d => d.DepartmentName).ToListAsync();

    public async Task<Department?> GetByIdAsync(int id) =>
        await _db.Departments.FindAsync(id);

    public async Task<Department> CreateAsync(Department department)
    {
        _db.Departments.Add(department);
        await _db.SaveChangesAsync();
        return department;
    }

    public async Task<bool> UpdateAsync(Department department)
    {
        _db.Departments.Update(department);
        return await _db.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var department = await _db.Departments.FindAsync(id);
        if (department == null) return false;
        
        _db.Departments.Remove(department);
        return await _db.SaveChangesAsync() > 0;
    }
}
