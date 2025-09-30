using Microsoft.EntityFrameworkCore;
using ModularSys.Data.Common.Db;
using ModularSys.Data.Common.Entities;
using ModularSys.Core.Interfaces;
using System.Security.Cryptography;
using System.Text;

public class UserService : IUserService
{
    private readonly ModularSysDbContext _db;
    private readonly IAuthService _authService;

    public UserService(ModularSysDbContext db, IAuthService authService)
    {
        _db = db;
        _authService = authService;
    }

    public async Task<List<User>> GetAllAsync() =>
        await _db.Users.Include(u => u.Role).Include(u => u.Department).ToListAsync();

    public async Task<User?> GetByIdAsync(int id) =>
        await _db.Users.Include(u => u.Role).Include(u => u.Department).FirstOrDefaultAsync(u => u.Id == id);

    public async Task<User?> GetByUsernameAsync(string username) =>
        await _db.Users.Include(u => u.Role).Include(u => u.Department).FirstOrDefaultAsync(u => u.Username == username);

    public async Task<User> CreateAsync(User user, string password)
    {
        user.PasswordHash = HashPassword(password);
        user.CreatedAt = DateTime.UtcNow;
        user.CreatedBy = _authService.CurrentUser ?? "System";
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return user;
    }

    public async Task<bool> UpdateAsync(User user)
    {
        user.UpdatedAt = DateTime.UtcNow;
        user.UpdatedBy = _authService.CurrentUser ?? "System";
        _db.Users.Update(user);
        return await _db.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return false;
        
        // Soft delete manually
        user.IsDeleted = true;
        user.DeletedAt = DateTime.UtcNow;
        user.DeletedBy = _authService.CurrentUser;
        
        return await _db.SaveChangesAsync() > 0;
    }

    public async Task<bool> RestoreAsync(int id)
    {
        var user = await _db.Users.IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == id);
        if (user == null) return false;
        
        user.IsDeleted = false;
        user.DeletedAt = null;
        user.DeletedBy = null;
        
        return await _db.SaveChangesAsync() > 0;
    }

    public async Task<List<User>> GetDeletedUsersAsync() =>
        await _db.Users.IgnoreQueryFilters()
            .Where(u => u.IsDeleted)
            .Include(u => u.Role)
            .Include(u => u.Department)
            .ToListAsync();

    public async Task<(List<User> Users, int TotalCount)> GetPagedAsync(int page, int pageSize, string? searchTerm = null)
    {
        var query = _db.Users.Include(u => u.Role).Include(u => u.Department).AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(u => u.Username.Contains(searchTerm) || 
                                   u.FirstName.Contains(searchTerm) ||
                                   u.LastName.Contains(searchTerm) ||
                                   u.Email.Contains(searchTerm) ||
                                   (u.ContactNumber != null && u.ContactNumber.Contains(searchTerm)) ||
                                   u.Role.RoleName.Contains(searchTerm) ||
                                   u.Department.DepartmentName.Contains(searchTerm));
        }

        var totalCount = await query.CountAsync();
        var users = await query
            .OrderBy(u => u.Username)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (users, totalCount);
    }

    public async Task<bool> SetRoleAsync(int userId, int roleId)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user == null) return false;
        user.RoleId = roleId;
        _db.Users.Update(user);
        return await _db.SaveChangesAsync() > 0;
    }

    public async Task<bool> SetDepartmentAsync(int userId, int departmentId)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user == null) return false;
        user.DepartmentId = departmentId;
        _db.Users.Update(user);
        return await _db.SaveChangesAsync() > 0;
    }
    public async Task<int?> GetRoleIdAsync(int userId)
    {
        var user = await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId);

        return user?.RoleId;
    }

    private string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
    }

    public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user == null) return false;

        // Verify current password
        if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
            return false;

        // Update to new password
        user.PasswordHash = HashPassword(newPassword);
        user.UpdatedAt = DateTime.UtcNow;
        user.UpdatedBy = _authService.CurrentUser ?? "System";

        return await _db.SaveChangesAsync() > 0;
    }
}
