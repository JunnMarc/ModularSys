using Microsoft.EntityFrameworkCore;
using ModularSys.Core.Interfaces;
using ModularSys.Data.Common.Db;
using ModularSys.Data.Common.Entities;
using System.Security.Cryptography;
using System.Text;

namespace ModularSys.Core.Services
{
    public class AuthService : IAuthService
    {
        private readonly ModularSysDbContext _db;
        private readonly ISessionStorage _storage;

        public AuthService(ModularSysDbContext db, ISessionStorage storage)
        {
            _db = db;
            _storage = storage;

            CurrentUser = _storage.Get("current_user");
            IsAuthenticated = !string.IsNullOrEmpty(CurrentUser);
        }

        public bool IsAuthenticated { get; private set; }
        public string? CurrentUser { get; private set; }
        public event Action? OnAuthStateChanged;

        public async Task<bool> RegisterAsync(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || username.Length < 4)
                return false;

            if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
                return false;

            if (await _db.Users.AnyAsync(u => u.Username == username))
                return false;

            var defaultRole = await _db.Roles.FirstOrDefaultAsync(r => r.RoleName == "User");
            if (defaultRole == null)
                return false;

            var defaultDepartment = await _db.Departments.FirstOrDefaultAsync(d => d.DepartmentName == "Default");
            if (defaultDepartment == null)
                return false;

            var user = new User
            {
                Username = username,
                PasswordHash = HashPassword(password),
                Email = $"{username}@example.com",
                RoleId = defaultRole.RoleId,
                DepartmentId = defaultDepartment.DepartmentId,
                CreatedAt = DateTime.UtcNow,
                Role = defaultRole,
                Department = defaultDepartment
            };

            try
            {
                _db.Users.Add(user);
                await _db.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> LoginAsync(string username, string password)
        {
            var hash = HashPassword(password);
            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.Username == username && u.PasswordHash == hash);

            if (user != null)
            {
                IsAuthenticated = true;
                CurrentUser = user.Username;
                _storage.Set("current_user", user.Username);
                OnAuthStateChanged?.Invoke();
                return true;
            }

            IsAuthenticated = false;
            CurrentUser = null;
            _storage.Remove("current_user");
            OnAuthStateChanged?.Invoke();
            return false;
        }

        public void Logout()
        {
            IsAuthenticated = false;
            CurrentUser = null;
            _storage.Remove("current_user");
            OnAuthStateChanged?.Invoke();
        }

        private string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}
