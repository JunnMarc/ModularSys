using Microsoft.AspNetCore.Components.Authorization;
using ModularSys.Core.Security;

public class PermissionChecker : IPermissionChecker
{
    private readonly IUserService _userService;
    private readonly IRolePermissionService _rolePermissionService;

    public PermissionChecker(IUserService userService, IRolePermissionService rolePermissionService)
    {
        _userService = userService;
        _rolePermissionService = rolePermissionService;
    }

    public async Task<bool> HasPermissionAsync(int userId, string permissionName)
    {
        var roleId = await _userService.GetRoleIdAsync(userId);
        if (roleId == null) return false;

        var permissions = await _rolePermissionService.GetPermissionsForRoleAsync(roleId.Value);
        return permissions.Any(p => p.PermissionName == permissionName);
    }

    public async Task<List<string>> GetPermissionsAsync(int userId)
    {
        var roleId = await _userService.GetRoleIdAsync(userId);
        if (roleId == null) return new List<string>();

        var permissions = await _rolePermissionService.GetPermissionsForRoleAsync(roleId.Value);
        return permissions.Select(p => p.PermissionName).ToList();
    }
}
