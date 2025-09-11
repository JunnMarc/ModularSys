using Microsoft.EntityFrameworkCore;
using ModularSys.Core.Interfaces;
using ModularSys.Core.Security; // for SessionAuthStateProvider
using ModularSys.Data.Common.Db;
using ModularSys.Data.Common.Entities;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace ModularSys.Core.Services
{
    public class AuthService : IAuthService
    {
        private readonly ModularSysDbContext _db;
        private readonly ISessionStorage _storage;
        private readonly IRolePermissionService _rolePermissionService;
        private readonly SessionAuthStateProvider _authStateProvider;

        public AuthService(
            ModularSysDbContext db,
            ISessionStorage storage,
            IRolePermissionService rolePermissionService,
            SessionAuthStateProvider authStateProvider)
        {
            _db = db;
            _storage = storage;
            _rolePermissionService = rolePermissionService;
            _authStateProvider = authStateProvider;
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
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Username == username && u.PasswordHash == hash);

            if (user != null)
            {
                var permissions = await _rolePermissionService.GetPermissionsForRoleAsync(user.RoleId);

                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Role, user.Role.RoleName)
        };

                foreach (var perm in permissions)
                    claims.Add(new Claim("Permission", perm.PermissionName));

                _storage.Set("current_user", user.Username);
                _storage.SetClaims("current_user_claims", claims);

                IsAuthenticated = true;
                CurrentUser = user.Username;

                _authStateProvider.NotifyUserAuthentication(claims);
                OnAuthStateChanged?.Invoke();
                return true;
            }

            _storage.Remove("current_user");
            _storage.Remove("current_user_claims");

            IsAuthenticated = false;
            CurrentUser = null;

            _authStateProvider.NotifyUserLogout();
            OnAuthStateChanged?.Invoke();
            return false;
        }

        public async Task<bool> RefreshClaimsAsync()
        {
            var principal = _authStateProvider.GetAuthenticationStateAsync().Result.User;

            if (principal.Identity?.IsAuthenticated != true)
                return false;

            var idValue = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(idValue) || !int.TryParse(idValue, out var userId))
                return false;

            var dbUser = _db.Users
                .Include(u => u.Role)
                .FirstOrDefault(u => u.Id == userId);

            if (dbUser == null)
                return false;

            var permissions = _rolePermissionService.GetPermissionsForRoleAsync(dbUser.RoleId).Result;

            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, dbUser.Username),
        new Claim(ClaimTypes.NameIdentifier, dbUser.Id.ToString()),
        new Claim(ClaimTypes.Role, dbUser.Role.RoleName)
    };

            foreach (var perm in permissions)
                claims.Add(new Claim("Permission", perm.PermissionName));

            _storage.Set("current_user", dbUser.Username);
            _storage.SetClaims("current_user_claims", claims);

            _authStateProvider.NotifyUserAuthentication(claims);
            OnAuthStateChanged?.Invoke();

            IsAuthenticated = true;
            CurrentUser = dbUser.Username;

            return true;
        }

        public async void Logout()
        {
            _storage.Remove("current_user");
            _storage.Remove("current_user_claims");

            IsAuthenticated = false;
            CurrentUser = null;

            _authStateProvider.NotifyUserLogout();
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
