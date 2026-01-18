using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace GrcMvc.PluginFramework
{
    /// <summary>
    /// Service responsible for discovering and loading modules
    /// </summary>
    public class ModuleLoader : IModuleLoader
    {
        private readonly ILogger<ModuleLoader> _logger;
        private readonly Dictionary<string, IGrcModule> _modules;
        private readonly Dictionary<string, Assembly> _moduleAssemblies;
        private readonly IServiceProvider _serviceProvider;

        public ModuleLoader(ILogger<ModuleLoader> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _modules = new Dictionary<string, IGrcModule>();
            _moduleAssemblies = new Dictionary<string, Assembly>();
        }

        /// <summary>
        /// Get all loaded modules
        /// </summary>
        public IReadOnlyDictionary<string, IGrcModule> LoadedModules => _modules;

        /// <summary>
        /// Discover modules from a directory
        /// </summary>
        public async Task<IEnumerable<IGrcModule>> DiscoverModulesAsync(string modulesPath)
        {
            var discoveredModules = new List<IGrcModule>();

            try
            {
                if (!Directory.Exists(modulesPath))
                {
                    _logger.LogWarning($"Modules directory not found: {modulesPath}");
                    return discoveredModules;
                }

                var moduleFiles = Directory.GetFiles(modulesPath, "GrcMvc.Modules.*.dll", SearchOption.AllDirectories);
                _logger.LogInformation($"Found {moduleFiles.Length} potential module assemblies");

                foreach (var moduleFile in moduleFiles)
                {
                    try
                    {
                        var module = await LoadModuleFromAssemblyAsync(moduleFile);
                        if (module != null)
                        {
                            discoveredModules.Add(module);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Failed to load module from {moduleFile}");
                    }
                }

                return discoveredModules;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error discovering modules");
                return discoveredModules;
            }
        }

        /// <summary>
        /// Load a module from an assembly file
        /// </summary>
        public async Task<IGrcModule> LoadModuleFromAssemblyAsync(string assemblyPath)
        {
            try
            {
                _logger.LogDebug($"Loading module from: {assemblyPath}");

                // Load assembly
                var assembly = Assembly.LoadFrom(assemblyPath);
                
                // Find module types
                var moduleTypes = assembly.GetTypes()
                    .Where(t => typeof(IGrcModule).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface)
                    .ToList();

                if (!moduleTypes.Any())
                {
                    _logger.LogWarning($"No module types found in {assemblyPath}");
                    return null;
                }

                if (moduleTypes.Count > 1)
                {
                    _logger.LogWarning($"Multiple module types found in {assemblyPath}, using first one");
                }

                // Create module instance
                var moduleType = moduleTypes.First();
                var module = Activator.CreateInstance(moduleType) as IGrcModule;

                if (module != null)
                {
                    _moduleAssemblies[module.Id] = assembly;
                    _logger.LogInformation($"Loaded module: {module.Name} v{module.Version} from {Path.GetFileName(assemblyPath)}");
                }

                return module;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to load module from {assemblyPath}");
                return null;
            }
        }

        /// <summary>
        /// Initialize all discovered modules
        /// </summary>
        public async Task InitializeModulesAsync(IEnumerable<IGrcModule> modules)
        {
            // Sort modules by priority
            var sortedModules = modules.OrderBy(m => m.Priority).ThenBy(m => m.Name).ToList();
            
            _logger.LogInformation($"Initializing {sortedModules.Count} modules");

            foreach (var module in sortedModules)
            {
                try
                {
                    // Check dependencies
                    if (!await CheckDependenciesAsync(module))
                    {
                        _logger.LogError($"Module {module.Name} has unmet dependencies");
                        continue;
                    }

                    // Initialize module
                    await module.OnStartupAsync(_serviceProvider);
                    
                    // Register module
                    _modules[module.Id] = module;
                    
                    _logger.LogInformation($"Module {module.Name} initialized successfully");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to initialize module: {module.Name}");
                }
            }
        }

        /// <summary>
        /// Shutdown all modules
        /// </summary>
        public async Task ShutdownModulesAsync()
        {
            _logger.LogInformation("Shutting down all modules");

            // Shutdown in reverse order
            var modulesToShutdown = _modules.Values
                .OrderByDescending(m => m.Priority)
                .ThenByDescending(m => m.Name)
                .ToList();

            foreach (var module in modulesToShutdown)
            {
                try
                {
                    await module.OnShutdownAsync();
                    _logger.LogInformation($"Module {module.Name} shut down successfully");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error shutting down module: {module.Name}");
                }
            }

            _modules.Clear();
            _moduleAssemblies.Clear();
        }

        /// <summary>
        /// Get a loaded module by ID
        /// </summary>
        public IGrcModule GetModule(string moduleId)
        {
            return _modules.TryGetValue(moduleId, out var module) ? module : null;
        }

        /// <summary>
        /// Check if a module is loaded
        /// </summary>
        public bool IsModuleLoaded(string moduleId)
        {
            return _modules.ContainsKey(moduleId);
        }

        /// <summary>
        /// Check module dependencies
        /// </summary>
        private async Task<bool> CheckDependenciesAsync(IGrcModule module)
        {
            var dependencies = module.GetDependencies();
            
            foreach (var dependency in dependencies)
            {
                if (!IsModuleLoaded(dependency.ModuleId))
                {
                    if (dependency.IsRequired)
                    {
                        _logger.LogError($"Module {module.Name} requires {dependency.ModuleId} which is not loaded");
                        return false;
                    }
                    else
                    {
                        _logger.LogWarning($"Module {module.Name} optionally depends on {dependency.ModuleId} which is not loaded");
                    }
                }
                else
                {
                    var depModule = GetModule(dependency.ModuleId);
                    
                    // Check version compatibility
                    if (!IsVersionCompatible(depModule.Version, dependency.MinVersion, dependency.MaxVersion))
                    {
                        _logger.LogError($"Module {module.Name} requires {dependency.ModuleId} version between {dependency.MinVersion} and {dependency.MaxVersion}, but {depModule.Version} is loaded");
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Check if a version is compatible with min/max requirements
        /// </summary>
        private bool IsVersionCompatible(string version, string minVersion, string maxVersion)
        {
            if (!Version.TryParse(version, out var v))
                return false;

            if (!string.IsNullOrEmpty(minVersion) && Version.TryParse(minVersion, out var min))
            {
                if (v < min)
                    return false;
            }

            if (!string.IsNullOrEmpty(maxVersion) && Version.TryParse(maxVersion, out var max))
            {
                if (v > max)
                    return false;
            }

            return true;
        }
    }

    /// <summary>
    /// Interface for module loader
    /// </summary>
    public interface IModuleLoader
    {
        IReadOnlyDictionary<string, IGrcModule> LoadedModules { get; }
        Task<IEnumerable<IGrcModule>> DiscoverModulesAsync(string modulesPath);
        Task<IGrcModule> LoadModuleFromAssemblyAsync(string assemblyPath);
        Task InitializeModulesAsync(IEnumerable<IGrcModule> modules);
        Task ShutdownModulesAsync();
        IGrcModule GetModule(string moduleId);
        bool IsModuleLoaded(string moduleId);
    }
}
