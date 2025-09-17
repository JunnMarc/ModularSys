using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModularSys.Core.Interfaces;

namespace ModularSys.Core.Services;

public interface IStartupSeedingService
{
    Task SeedInitialDataAsync();
}

public class StartupSeedingService : IStartupSeedingService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<StartupSeedingService> _logger;

    public StartupSeedingService(IServiceProvider serviceProvider, ILogger<StartupSeedingService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task SeedInitialDataAsync()
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var permissionSeedingService = scope.ServiceProvider.GetRequiredService<IPermissionSeedingService>();

            _logger.LogInformation("Starting initial data seeding...");

            // Seed permissions first
            await permissionSeedingService.SeedPermissionsAsync();
            
            // Then seed role templates
            await permissionSeedingService.SeedRoleTemplatesAsync();

            _logger.LogInformation("Initial data seeding completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during initial data seeding");
            throw;
        }
    }
}
