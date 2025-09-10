using ModularSys.Data.Common.Entities;

public interface IPermissionService
{
    Task<List<Permission>> GetAllAsync();
    Task<Permission?> GetByIdAsync(int id);
    Task<Permission> CreateAsync(Permission permission);
    Task<bool> UpdateAsync(Permission permission);
    Task<bool> DeleteAsync(int id);
}
