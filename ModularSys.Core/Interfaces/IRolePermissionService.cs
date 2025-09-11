using ModularSys.Data.Common.Entities;

public interface IRolePermissionService
{
    Task<List<RolePermission>> GetAllAsync();
    Task<RolePermission?> GetAsync(int roleId, int permissionId);
    Task<RolePermission> CreateAsync(RolePermission rolePermission);
    Task<bool> DeleteAsync(int roleId, int permissionId);

    Task<bool> ToggleAsync(int roleId, int permissionId, bool isGranted);
    Task AssignPermissionsAsync(int roleId, List<int> permissionIds);
    Task<List<Permission>> GetPermissionsForRoleAsync(int roleId);
    Task<List<Permission>> GetPermissionsForRolesAsync(IEnumerable<int> roleIds);
}

