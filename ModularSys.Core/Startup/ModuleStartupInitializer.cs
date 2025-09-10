using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModularSys.Core.Loader;

public interface IStartupInitializer
{
    void Initialize();
}

public class ModuleStartupInitializer : IStartupInitializer
{
    private readonly ILogger<ModuleStartupInitializer> _logger;
    private readonly IServiceCollection _services;

    public ModuleStartupInitializer(ILogger<ModuleStartupInitializer> logger, IServiceCollection services)
    {
        _logger = logger;
        _services = services;
        Initialize();
    }

    public void Initialize()
    {
        ModuleLoader.RegisterAllModules(_services, _logger);
    }
}
