using ModularSys.Data.Common.Entities;

public interface IRolePermissionService
{
    Task<List<RolePermission>> GetAllAsync();
    Task<RolePermission?> GetAsync(int roleId, int permissionId);
    Task<RolePermission> CreateAsync(RolePermission rolePermission);
    Task<bool> DeleteAsync(int roleId, int permissionId);
}
