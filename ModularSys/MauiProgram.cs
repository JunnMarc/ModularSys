using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ModularSys.Core.Extensions;
using ModularSys.Core.Interfaces;
using ModularSys.Core.Loader;
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

            //Initializing SQL Server
            builder.Services.AddDbContext<ModularSysDbContext>(options =>
                options.UseSqlServer(connectionString));

            // Auth + session
            builder.Services.AddSingleton<ISessionStorage, MauiSessionStorage>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddCoreServices();


            // User Management services
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IRoleService, RoleService>();
            builder.Services.AddScoped<IPermissionService, PermissionService>();
            builder.Services.AddScoped<IRolePermissionService, RolePermissionService>();

            // ✅ Register dynamic modules before building the app
            var loggerFactory = LoggerFactory.Create(config => config.AddDebug());
            var logger = loggerFactory.CreateLogger("ModuleLoader");
            ModuleLoader.RegisterAllModules(builder.Services, logger);

            return builder.Build();
        }
    }
}
