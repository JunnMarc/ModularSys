using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModularSys.Core.Interfaces;
using System.Reflection;
using System.Runtime.Loader;

namespace ModularSys.Core.Loader;

public static class ModuleLoader
{
    private static readonly List<ISubsystem> _subsystems = new();
    private static readonly List<Assembly> _assemblies = new();

    public static IReadOnlyList<ISubsystem> Subsystems => _subsystems;
    public static IReadOnlyList<Assembly> Assemblies => _assemblies;

    public static void RegisterAllModules(IServiceCollection services, ILogger? logger = null)
    {
        _subsystems.Clear();
        _assemblies.Clear();

        var basePath = AppContext.BaseDirectory;

        var dlls = Directory.GetFiles(basePath, "ModuERP.*.dll", SearchOption.TopDirectoryOnly)
            .Where(f => !f.Contains("ModuERP.Core"));

        logger?.LogInformation("[ModuleLoader] Scanning directory: {Path}", basePath);
        foreach (var dll in dlls)
        {
            logger?.LogInformation("[ModuleLoader] Found DLL: {Dll}", Path.GetFileName(dll));
        }


        foreach (var dll in dlls)
        {
            try
            {
                var asm = Assembly.LoadFrom(dll);
                _assemblies.Add(asm);
                LoadSubsystemsFromAssembly(asm, services, logger);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "[ModuleLoader] Failed to load {Dll}", dll);
            }
        }
    }

    private static void LoadSubsystemsFromAssembly(Assembly asm, IServiceCollection services, ILogger? logger)
    {
        var subsystems = asm.GetTypes()
            .Where(t => typeof(ISubsystem).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

        logger?.LogInformation("[ModuleLoader] Inspecting assembly: {Assembly}", asm.FullName);
        foreach (var type in asm.GetTypes())
        {
            logger?.LogDebug("[ModuleLoader] Found type: {Type}", type.FullName);
        }

        foreach (var type in subsystems)
        {
            try
            {
                var subsystem = (ISubsystem)Activator.CreateInstance(type)!;
                _subsystems.Add(subsystem);
                subsystem.RegisterServices(services);

                logger?.LogInformation("[ModuleLoader] Loaded subsystem: {Name} ({Route})", subsystem.Name, subsystem.Route);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "[ModuleLoader] Failed to initialize subsystem {Type}", type.FullName);
            }
        }
    }
}
