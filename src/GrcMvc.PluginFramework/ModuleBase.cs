using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GrcMvc.PluginFramework
{
    /// <summary>
    /// Base implementation for GRC modules
    /// </summary>
    public abstract class ModuleBase : IGrcModule
    {
        protected ILogger Logger { get; private set; }
        
        public abstract string Id { get; }
        public abstract string Name { get; }
        public abstract string Version { get; }
        public virtual string Description { get; } = string.Empty;
        public virtual string Author { get; } = "GRC Team";
        public virtual ModulePriority Priority { get; } = ModulePriority.Normal;
        public ModuleStatus Status { get; private set; } = ModuleStatus.NotLoaded;

        /// <summary>
        /// Configure module services
        /// </summary>
        public virtual void ConfigureServices(IServiceCollection services)
        {
            // Override in derived classes
        }

        /// <summary>
        /// Configure middleware pipeline
        /// </summary>
        public virtual void Configure(IApplicationBuilder app)
        {
            // Override in derived classes
        }

        /// <summary>
        /// Configure database entities for this module
        /// </summary>
        public virtual void ConfigureDatabase(ModelBuilder modelBuilder)
        {
            // Override in derived classes
        }

        /// <summary>
        /// Module startup logic
        /// </summary>
        public virtual async Task OnStartupAsync(IServiceProvider serviceProvider)
        {
            try
            {
                Status = ModuleStatus.Starting;
                
                // Get logger
                var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
                if (loggerFactory != null)
                {
                    Logger = loggerFactory.CreateLogger(GetType());
                    Logger.LogInformation($"Starting module: {Name} v{Version}");
                }

                // Validate configuration
                var validationResult = ValidateConfiguration();
                if (!validationResult.IsValid)
                {
                    Status = ModuleStatus.Failed;
                    var errors = string.Join(", ", validationResult.Errors);
                    Logger?.LogError($"Module {Name} validation failed: {errors}");
                    throw new InvalidOperationException($"Module validation failed: {errors}");
                }

                // Check dependencies
                var dependencies = GetDependencies();
                foreach (var dep in dependencies.Where(d => d.IsRequired))
                {
                    Logger?.LogDebug($"Checking dependency: {dep.ModuleId}");
                    // Module loader will handle dependency checking
                }

                // Perform module-specific startup
                await OnModuleStartupAsync(serviceProvider);

                Status = ModuleStatus.Running;
                Logger?.LogInformation($"Module {Name} started successfully");
            }
            catch (Exception ex)
            {
                Status = ModuleStatus.Failed;
                Logger?.LogError(ex, $"Failed to start module: {Name}");
                throw;
            }
        }

        /// <summary>
        /// Module shutdown logic
        /// </summary>
        public virtual async Task OnShutdownAsync()
        {
            try
            {
                Status = ModuleStatus.Stopping;
                Logger?.LogInformation($"Stopping module: {Name}");

                await OnModuleShutdownAsync();

                Status = ModuleStatus.Stopped;
                Logger?.LogInformation($"Module {Name} stopped successfully");
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, $"Error stopping module: {Name}");
                throw;
            }
        }

        /// <summary>
        /// Get module dependencies
        /// </summary>
        public virtual IEnumerable<ModuleDependency> GetDependencies()
        {
            return Enumerable.Empty<ModuleDependency>();
        }

        /// <summary>
        /// Get types exported by this module
        /// </summary>
        public virtual IEnumerable<Type> GetExportedTypes()
        {
            return GetType().Assembly.GetExportedTypes();
        }

        /// <summary>
        /// Validate module configuration
        /// </summary>
        public virtual ValidationResult ValidateConfiguration()
        {
            return ValidationResult.Success();
        }

        /// <summary>
        /// Get module capabilities
        /// </summary>
        public virtual IEnumerable<ModuleCapability> GetCapabilities()
        {
            return Enumerable.Empty<ModuleCapability>();
        }

        /// <summary>
        /// Override for module-specific startup logic
        /// </summary>
        protected virtual Task OnModuleStartupAsync(IServiceProvider serviceProvider)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Override for module-specific shutdown logic
        /// </summary>
        protected virtual Task OnModuleShutdownAsync()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Helper method to register services with lifetime
        /// </summary>
        protected void RegisterService<TInterface, TImplementation>(IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Scoped)
            where TInterface : class
            where TImplementation : class, TInterface
        {
            switch (lifetime)
            {
                case ServiceLifetime.Singleton:
                    services.AddSingleton<TInterface, TImplementation>();
                    break;
                case ServiceLifetime.Transient:
                    services.AddTransient<TInterface, TImplementation>();
                    break;
                default:
                    services.AddScoped<TInterface, TImplementation>();
                    break;
            }
            
            Logger?.LogDebug($"Registered service: {typeof(TInterface).Name} -> {typeof(TImplementation).Name}");
        }
    }
}
