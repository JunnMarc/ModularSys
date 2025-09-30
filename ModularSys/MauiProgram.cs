using ApexCharts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModularSys.Core.Extensions;
using ModularSys.Core.Interfaces;
using ModularSys.Core.Loader;
using ModularSys.Core.Security;
using ModularSys.Core.Services;
using ModularSys.Data.Common.Db;
using ModularSys.Data.Common.Services.Sync;
using MudBlazor.Services;

namespace ModularSys
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();

            //Mudblazor Services
            builder.Services.AddMudServices();
            builder.Services.AddMudBlazorDialog();
            builder.Services.AddMudBlazorSnackbar();
            builder.Services.AddMudBlazorResizeListener();
            builder.Services.AddApexCharts();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#endif
            // Load appsettings.json and sync configuration
            builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            builder.Configuration.AddJsonFile("appsettings.Sync.json", optional: true, reloadOnChange: true);
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

            // Database
            // 1) Scoped DbContext for normal page/services (optional but common)
            builder.Services.AddDbContext<ModularSysDbContext>(options =>
                options.UseSqlServer(connectionString, sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                })
                .ConfigureWarnings(warnings => warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning)));
            
            builder.Services.AddDbContext<InventoryDbContext>(options =>
                options.UseSqlServer(connectionString, sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                }));

            // 2) Factory for isolated contexts (policy checks, background ops)
            builder.Services.AddDbContextFactory<ModularSysDbContext>(options =>
                options.UseSqlServer(connectionString, sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                }));
            
            builder.Services.AddDbContextFactory<InventoryDbContext>(options =>
                options.UseSqlServer(connectionString, sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                }));

            // Session & Auth
            builder.Services.AddAuthorizationCore();

            // Auth state provider (single instance registered as itself and as the interface)
            builder.Services.AddScoped<SessionAuthStateProvider>();
            builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<SessionAuthStateProvider>());

            // Dynamic policies
            builder.Services.AddScoped<IAuthorizationPolicyProvider, DynamicAuthPolicyProvider>();

            // Session storage
            builder.Services.AddScoped<ISessionStorage, MauiSessionStorage>();

            // Domain services
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IRolePermissionService, RolePermissionService>();
            builder.Services.AddScoped<IPermissionService, PermissionService>();
            builder.Services.AddScoped<IClaimsPermissionChecker, ClaimsPermissionChecker>();
            builder.Services.AddScoped<IPermissionChecker, PermissionChecker>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IRoleService, RoleService>();
            builder.Services.AddScoped<IDepartmentService, DepartmentService>();
            builder.Services.AddScoped<IDashboardService, DashboardService>();
            builder.Services.AddScoped<IPermissionSeedingService, PermissionSeedingService>();
            builder.Services.AddScoped<IStartupSeedingService, StartupSeedingService>();
            builder.Services.AddScoped<ISoftDeleteService, SoftDeleteService>();
            
            // Sync services (offline-first synchronization)
            builder.Services.AddSyncServices(builder.Configuration);

            // HttpClient (adjust BaseAddress to your API host/port)
            builder.Services.AddScoped(sp => new HttpClient
            {
                BaseAddress = new Uri("https://localhost:5001/") // Desktop: API running locally
            });

            // Register dynamic modules
            var loggerFactory = LoggerFactory.Create(config => config.AddDebug());
            var logger = loggerFactory.CreateLogger("ModuleLoader");
            ModuleLoader.RegisterAllModules(builder.Services, logger);

            var app = builder.Build();

            // Seed initial data on startup
            Task.Run(async () =>
            {
                try
                {
                    using var scope = app.Services.CreateScope();
                    var seedingService = scope.ServiceProvider.GetRequiredService<IStartupSeedingService>();
                    await seedingService.SeedInitialDataAsync();
                }
                catch (Exception ex)
                {
                    var startupLogger = loggerFactory.CreateLogger("Startup");
                    startupLogger.LogError(ex, "Failed to seed initial data");
                }
            });

            return app;
        }
    }
}
