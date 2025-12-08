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

    public static void AddAssembly(Assembly assembly)
    {
        if (!_assemblies.Contains(assembly))
        {
            _assemblies.Add(assembly);
        }
    }

    public static void RegisterAllModules(IServiceCollection services, ILogger? logger = null)
    {
        _subsystems.Clear();
        _assemblies.Clear();

        // 1. Check already loaded assemblies (referenced projects)
        var loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => a.FullName != null && a.FullName.StartsWith("ModularSys.") && !a.FullName.Contains("ModularSys.Core"));

        foreach (var asm in loadedAssemblies)
        {
            if (!_assemblies.Contains(asm))
            {
                System.Diagnostics.Debug.WriteLine($"[ModuleLoader] Found Loaded Assembly: {asm.FullName}");
                _assemblies.Add(asm);
                LoadSubsystemsFromAssembly(asm, services, logger);
            }
        }

        // 2. Scan directory for dynamic plugins (if any)
        var basePath = AppContext.BaseDirectory;
        var dlls = Directory.GetFiles(basePath, "ModularSys.*.dll", SearchOption.TopDirectoryOnly)
            .Where(f => !f.Contains("ModularSys.Core"));

        logger?.LogInformation("[ModuleLoader] Scanning directory: {Path}", basePath);
        
        foreach (var dll in dlls)
        {
            try
            {
                var fileName = Path.GetFileNameWithoutExtension(dll);
                if (_assemblies.Any(a => a.GetName().Name == fileName))
                    continue; // Already loaded

                var asm = Assembly.LoadFrom(dll);
                System.Diagnostics.Debug.WriteLine($"[ModuleLoader] Loaded Assembly from File: {asm.FullName}");
                _assemblies.Add(asm);
                LoadSubsystemsFromAssembly(asm, services, logger);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ModuleLoader] Failed to load {dll}: {ex}");
                logger?.LogError(ex, "[ModuleLoader] Failed to load {Dll}", dll);
            }
        }
        System.Diagnostics.Debug.WriteLine($"[ModuleLoader] Total Assemblies: {_assemblies.Count}");
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
