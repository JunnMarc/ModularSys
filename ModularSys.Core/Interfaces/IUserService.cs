using ModularSys.Data.Common.Entities;

public interface IUserService
{
    Task<List<User>> GetAllAsync();
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByUsernameAsync(string username);
    Task<User> CreateAsync(User user, string password);
    Task<bool> UpdateAsync(User user);
    Task<bool> DeleteAsync(int id);
    Task<bool> RestoreAsync(int id);
    Task<List<User>> GetDeletedUsersAsync();
    Task<(List<User> Users, int TotalCount)> GetPagedAsync(int page, int pageSize, string? searchTerm = null);
    Task<bool> SetRoleAsync(int userId, int roleId);
    Task<bool> SetDepartmentAsync(int userId, int departmentId);
    Task<int?> GetRoleIdAsync(int userId);
    Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
}
