using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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



            return builder.Build();
        }
    }
}
