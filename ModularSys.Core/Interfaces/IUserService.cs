using ModularSys.Data.Common.Entities;

public interface IUserService
{
    Task<List<User>> GetAllAsync();
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByUsernameAsync(string username);
    Task<User> CreateAsync(User user, string password);
    Task<bool> UpdateAsync(User user);
    Task<bool> DeleteAsync(int id);
    Task<bool> SetRoleAsync(int userId, int roleId);
    Task<bool> SetDepartmentAsync(int userId, int departmentId);
}
