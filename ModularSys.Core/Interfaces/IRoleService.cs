using ModularSys.Data.Common.Entities;

public interface IRoleService
{
    Task<List<Role>> GetAllAsync();
    Task<Role?> GetByIdAsync(int id);
    Task<Role> CreateAsync(Role role);
    Task<bool> UpdateAsync(Role role);
    Task<bool> DeleteAsync(int id);
}
