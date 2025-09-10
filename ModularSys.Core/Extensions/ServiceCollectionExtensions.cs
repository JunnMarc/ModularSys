using Microsoft.Extensions.DependencyInjection;
using ModularSys.Core.Interfaces;
using ModularSys.Core.Services;

namespace ModularSys.Core.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCoreServices(this IServiceCollection services)
        {
            services.AddScoped<IAuthService, AuthService>();
            return services;
        }
    }
}
