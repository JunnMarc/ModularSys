using Microsoft.EntityFrameworkCore;
using ModularSys.Data.Common.Db;
using ModularSys.Data.Common.Entities;
using System.Security.Cryptography;
using System.Text;

public class UserService : IUserService
{
    private readonly ModularSysDbContext _db;

    public UserService(ModularSysDbContext db)
    {
        _db = db;
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
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
        return user;
    }

    public async Task<bool> UpdateAsync(User user)
    {
        _db.Users.Update(user);
        return await _db.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return false;
        _db.Users.Remove(user);
        return await _db.SaveChangesAsync() > 0;
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

    private string HashPassword(string password)
    {
        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}
