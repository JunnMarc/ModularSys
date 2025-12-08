using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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
        private readonly Lazy<IAuditService> _auditService;

        public AuthService(
            IDbContextFactory<ModularSysDbContext> contextFactory,
            ISessionStorage storage,
            IRolePermissionService rolePermissionService,
            SessionAuthStateProvider authStateProvider,
            IServiceProvider serviceProvider)
        {
            _contextFactory = contextFactory;
            _storage = storage;
            _rolePermissionService = rolePermissionService;
            _authStateProvider = authStateProvider;
            _auditService = new Lazy<IAuditService>(() => serviceProvider.GetRequiredService<IAuditService>());
            
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
                CurrentUserRole = _storage.Get("current_user_role");
                
                var cid = _storage.Get("current_user_customer_id");
                if (int.TryParse(cid, out var id))
                    CurrentUserCustomerId = id;

                // DEBUG LOGGING
                System.Diagnostics.Debug.WriteLine($"[AuthService] Rehydrated State. User: {CurrentUser}, Role: {CurrentUserRole}");
            }
            else
            {
                IsAuthenticated = false;
                CurrentUser = null;
                CurrentUserRole = null;
                CurrentUserCustomerId = null;
            }
        }

        public bool IsAuthenticated { get; private set; }
        public string? CurrentUser { get; private set; }
        public string? CurrentUserRole { get; private set; }
        public int? CurrentUserCustomerId { get; private set; }
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

        public async Task<(bool Success, string Message)> RegisterCustomerAsync(string username, string password, string firstName, string lastName, string email)
        {
            if (string.IsNullOrWhiteSpace(username) || 
                string.IsNullOrWhiteSpace(password) || 
                string.IsNullOrWhiteSpace(firstName) || 
                string.IsNullOrWhiteSpace(lastName) || 
                string.IsNullOrWhiteSpace(email))
                return (false, "All fields are required.");

            await using var db = _contextFactory.CreateDbContext();
            var strategy = db.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await db.Database.BeginTransactionAsync();
                try 
                {
                    if (await db.Users.AnyAsync(u => u.Username == username))
                        return (false, "Username is already taken.");
                        
                    // Ensure Customer Role exists
                    var customerRole = await db.Roles.FirstOrDefaultAsync(r => r.RoleName == "Customer");
                    if (customerRole == null)
                    {
                        customerRole = new Role 
                        { 
                            RoleName = "Customer", 
                            Description = "External Customer Access",
                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = "System"
                        };
                        db.Roles.Add(customerRole);
                        await db.SaveChangesAsync();
                    }
                    
                    // Get Default Department (Sales or generic)
                    var dept = await db.Departments.FirstOrDefaultAsync() 
                               ?? new Department { DepartmentName = "Default", DepartmentDesc = "Default Department" };
                    
                    if (dept.DepartmentId == 0) // Was created
                    {
                         db.Departments.Add(dept);
                         await db.SaveChangesAsync();
                    }

                    // Create Customer CRM Entity
                    var customer = new ModularSys.Data.Common.Entities.CRM.Customer
                    {
                        CompanyName = $"{firstName} {lastName}", // Individual
                        ContactName = $"{firstName} {lastName}",
                        Email = email,
                        CustomerType = "Customer",
                        Status = "Active",
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = "System",
                        LoyaltyPoints = 0
                    };
                    
                    db.Customers.Add(customer);
                    await db.SaveChangesAsync();

                    var user = new User
                    {
                        Username = username,
                        PasswordHash = HashPassword(password),
                        Email = email,
                        FirstName = firstName,
                        LastName = lastName,
                        RoleId = customerRole.RoleId,
                        DepartmentId = dept.DepartmentId,
                        CustomerId = customer.Id,
                        CreatedAt = DateTime.UtcNow,
                        Role = customerRole,
                        Department = dept
                    };
                    
                    db.Users.Add(user);
                    await db.SaveChangesAsync();
                    
                    await transaction.CommitAsync();
                    
                    return (true, "Registration successful.");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    var msg = $"{ex.Message} {ex.InnerException?.Message}";
                    System.Diagnostics.Debug.WriteLine($"RegisterCustomerAsync Failed: {msg}");
                    return (false, $"Error: {msg}");
                }
            });
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
                _storage.Remove("current_user_role");
                _storage.Remove("current_user_customer_id");
                _storage.Remove("current_user_claims");

                IsAuthenticated = false;
                CurrentUser = null;
                CurrentUserRole = null;
                CurrentUserCustomerId = null;

                _authStateProvider.NotifyUserLogout();
                OnAuthStateChanged?.Invoke();
                
                // Log failed login attempt
                try
                {
                    await _auditService.Value.LogFailedLoginAsync(username, "Invalid username or password");
                }
                catch { /* Ignore audit logging errors */ }
                
                return false;
            }

            var permissions = await _rolePermissionService.GetPermissionsForRoleAsync(user.RoleId);
            
            // DEBUG LOGGING
            System.Diagnostics.Debug.WriteLine($"[AuthService] Login Successful. User: {user.Username}, Role: {user.Role?.RoleName}");

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role.RoleName)
            };

            if (user.CustomerId.HasValue)
            {
                claims.Add(new Claim("CustomerCustomerId", user.CustomerId.Value.ToString()));
            }

            foreach (var perm in permissions)
                claims.Add(new Claim("Permission", perm.PermissionName));

            _storage.Set("current_user", user.Username);
            _storage.Set("current_user_role", user.Role.RoleName);
            
            if (user.CustomerId.HasValue)
            {
                _storage.Set("current_user_customer_id", user.CustomerId.Value.ToString());
                CurrentUserCustomerId = user.CustomerId.Value;
            }
            else
            {
                _storage.Remove("current_user_customer_id");
                CurrentUserCustomerId = null;
            }

            _storage.SetClaims("current_user_claims", claims);

            IsAuthenticated = true;
            CurrentUser = user.Username;
            CurrentUserRole = user.Role.RoleName;

            _authStateProvider.NotifyUserAuthentication(claims);
            OnAuthStateChanged?.Invoke();
            
            // Log successful login
            try
            {
                await _auditService.Value.LogLoginAsync(username, true);
            }
            catch { /* Ignore audit logging errors */ }
            
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

            if (dbUser.CustomerId.HasValue)
            {
                claims.Add(new Claim("CustomerCustomerId", dbUser.CustomerId.Value.ToString()));
            }

            foreach (var perm in permissions)
                claims.Add(new Claim("Permission", perm.PermissionName));

            _storage.Set("current_user", dbUser.Username);
            _storage.Set("current_user_role", dbUser.Role.RoleName);
            
             if (dbUser.CustomerId.HasValue)
            {
                _storage.Set("current_user_customer_id", dbUser.CustomerId.Value.ToString());
                 CurrentUserCustomerId = dbUser.CustomerId.Value;
            }
            else
            {
                 _storage.Remove("current_user_customer_id");
                 CurrentUserCustomerId = null;
            }
            
            _storage.SetClaims("current_user_claims", claims);

            _authStateProvider.NotifyUserAuthentication(claims);
            OnAuthStateChanged?.Invoke();

            IsAuthenticated = true;
            CurrentUser = dbUser.Username;
            CurrentUserRole = dbUser.Role.RoleName;

            return true;
        }


        public async void Logout()
        {
            var username = CurrentUser;
            
            _storage.Remove("current_user");
            _storage.Remove("current_user_role");
            _storage.Remove("current_user_customer_id");
            _storage.Remove("current_user_claims");

            IsAuthenticated = false;
            CurrentUser = null;
            CurrentUserRole = null;
            CurrentUserCustomerId = null;

            _authStateProvider.NotifyUserLogout();
            OnAuthStateChanged?.Invoke();
            
            // Log logout
            if (!string.IsNullOrEmpty(username))
            {
                try
                {
                    await _auditService.Value.LogLogoutAsync(username);
                }
                catch { /* Ignore audit logging errors */ }
            }
        }

        public string HashPassword(string password)
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
