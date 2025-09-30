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
        private readonly IDbContextFactory<ModularSysDbContext> _contextFactory;
        private readonly ISessionStorage _storage;
        private readonly IRolePermissionService _rolePermissionService;
        private readonly SessionAuthStateProvider _authStateProvider;

        public AuthService(
            IDbContextFactory<ModularSysDbContext> contextFactory,
            ISessionStorage storage,
            IRolePermissionService rolePermissionService,
            SessionAuthStateProvider authStateProvider)
        {
            _contextFactory = contextFactory;
            _storage = storage;
            _rolePermissionService = rolePermissionService;
            _authStateProvider = authStateProvider;
            
            // Initialize authentication state from session storage
            InitializeAuthState();
        }

        private void InitializeAuthState()
        {
            var currentUser = _storage.Get("current_user");
            if (!string.IsNullOrEmpty(currentUser))
            {
                IsAuthenticated = true;
                CurrentUser = currentUser;
            }
            else
            {
                IsAuthenticated = false;
                CurrentUser = null;
            }
        }

        public bool IsAuthenticated { get; private set; }
        public string? CurrentUser { get; private set; }
        public event Action? OnAuthStateChanged;

        public async Task<bool> RegisterAsync(string username, string password)
        {
            await using var db = _contextFactory.CreateDbContext();
            
            if (string.IsNullOrWhiteSpace(username) || username.Length < 4)
                return false;

            if (string.IsNullOrWhiteSpace(password) || password.Length < 12)
                return false;

            if (await db.Users.AnyAsync(u => u.Username == username))
                return false;

            var defaultRole = await db.Roles.FirstOrDefaultAsync(r => r.RoleId == 1);
            if (defaultRole == null)
                return false;

            var defaultDepartment = await db.Departments.FirstOrDefaultAsync(d => d.DepartmentId == 1);
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
                db.Users.Add(user);
                await db.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> LoginAsync(string username, string password)
        {
            await using var db = _contextFactory.CreateDbContext();
            var user = await db.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user == null || !await VerifyPasswordWithMigrationAsync(password, user))
            {
                _storage.Remove("current_user");
                _storage.Remove("current_user_claims");

                IsAuthenticated = false;
                CurrentUser = null;

                _authStateProvider.NotifyUserLogout();
                OnAuthStateChanged?.Invoke();
                return false;
            }

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

        public async Task<bool> RefreshClaimsAsync()
        {
            var authState = await _authStateProvider.GetAuthenticationStateAsync();
            var principal = authState.User;

            if (principal.Identity?.IsAuthenticated != true)
                return false;

            var idValue = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(idValue) || !int.TryParse(idValue, out var userId))
                return false;

            await using var db = _contextFactory.CreateDbContext();
            var dbUser = await db.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (dbUser == null)
                return false;

            var permissions = await _rolePermissionService.GetPermissionsForRoleAsync(dbUser.RoleId);

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
            return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
        }

        private bool VerifyPassword(string password, string hash)
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(password, hash);
            }
            catch
            {
                return false;
            }
        }

        private async Task<bool> VerifyPasswordWithMigrationAsync(string password, User user)
        {
            // First try BCrypt (new format)
            if (VerifyPassword(password, user.PasswordHash))
            {
                return true;
            }

            // Fallback to SHA256 (old format) and auto-migrate
            var sha256Hash = HashPasswordSHA256(password);
            if (user.PasswordHash == sha256Hash)
            {
                // Auto-migrate to BCrypt
                await using var db = _contextFactory.CreateDbContext();
                user.PasswordHash = HashPassword(password);
                db.Users.Update(user);
                await db.SaveChangesAsync();
                return true;
            }

            return false;
        }

        private string HashPasswordSHA256(string password)
        {
            using var sha = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}
