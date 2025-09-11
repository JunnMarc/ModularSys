using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ModularSys.Core.Extensions;
using ModularSys.Core.Interfaces;
using ModularSys.Core.Loader;
using ModularSys.Core.Security;
using ModularSys.Core.Services;
using ModularSys.Data.Common.Db;
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

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#endif
            // Load appsettings.json
            builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

            // Database
            // 1) Scoped DbContext for normal page/services (optional but common)
            builder.Services.AddDbContext<ModularSysDbContext>(options =>
                options.UseSqlServer(connectionString));
            builder.Services.AddDbContext<InventoryDbContext>(options =>
                options.UseSqlServer(connectionString));

            // 2) Factory for isolated contexts (policy checks, background ops)
            builder.Services.AddDbContextFactory<ModularSysDbContext>(options =>
                options.UseSqlServer(connectionString));
            builder.Services.AddDbContextFactory<InventoryDbContext>(options =>
                options.UseSqlServer(connectionString));

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

            // HttpClient (adjust BaseAddress to your API host/port)
            builder.Services.AddScoped(sp => new HttpClient
            {
                BaseAddress = new Uri("https://localhost:5001/") // Desktop: API running locally
            });

            // Register dynamic modules
            var loggerFactory = LoggerFactory.Create(config => config.AddDebug());
            var logger = loggerFactory.CreateLogger("ModuleLoader");
            ModuleLoader.RegisterAllModules(builder.Services, logger);

            return builder.Build();
        }
    }
}
