using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GrcMvc.PluginFramework
{
    /// <summary>
    /// Base interface for all GRC modules
    /// </summary>
    public interface IGrcModule
    {
        /// <summary>
        /// Unique identifier for the module
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Display name of the module
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Module version
        /// </summary>
        string Version { get; }

        /// <summary>
        /// Module description
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Module author/vendor
        /// </summary>
        string Author { get; }

        /// <summary>
        /// Module priority for loading order
        /// </summary>
        ModulePriority Priority { get; }

        /// <summary>
        /// Module status
        /// </summary>
        ModuleStatus Status { get; }

        /// <summary>
        /// Configure services for dependency injection
        /// </summary>
        void ConfigureServices(IServiceCollection services);

        /// <summary>
        /// Configure middleware pipeline
        /// </summary>
        void Configure(IApplicationBuilder app);

        /// <summary>
        /// Configure database model for this module
        /// </summary>
        void ConfigureDatabase(ModelBuilder modelBuilder);

        /// <summary>
        /// Called when module is starting
        /// </summary>
        Task OnStartupAsync(IServiceProvider serviceProvider);

        /// <summary>
        /// Called when module is shutting down
        /// </summary>
        Task OnShutdownAsync();

        /// <summary>
        /// Get module dependencies
        /// </summary>
        IEnumerable<ModuleDependency> GetDependencies();

        /// <summary>
        /// Get exported types for discovery
        /// </summary>
        IEnumerable<Type> GetExportedTypes();

        /// <summary>
        /// Validate module configuration
        /// </summary>
        ValidationResult ValidateConfiguration();

        /// <summary>
        /// Get module capabilities/features
        /// </summary>
        IEnumerable<ModuleCapability> GetCapabilities();
    }

    /// <summary>
    /// Module loading priority
    /// </summary>
    public enum ModulePriority
    {
        Critical = 0,    // Core modules that must load first
        High = 100,      // Important feature modules
        Normal = 200,    // Standard modules
        Low = 300,       // Optional modules
        VeryLow = 400    // Modules that can load last
    }

    /// <summary>
    /// Module status
    /// </summary>
    public enum ModuleStatus
    {
        NotLoaded,
        Loading,
        Loaded,
        Starting,
        Running,
        Stopping,
        Stopped,
        Failed,
        Disabled
    }

    /// <summary>
    /// Module dependency definition
    /// </summary>
    public class ModuleDependency
    {
        public string ModuleId { get; set; }
        public string MinVersion { get; set; }
        public string MaxVersion { get; set; }
        public bool IsRequired { get; set; }

        public ModuleDependency(string moduleId, string minVersion = null, string maxVersion = null, bool isRequired = true)
        {
            ModuleId = moduleId;
            MinVersion = minVersion;
            MaxVersion = maxVersion;
            IsRequired = isRequired;
        }
    }

    /// <summary>
    /// Module capability/feature declaration
    /// </summary>
    public class ModuleCapability
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public bool IsEnabled { get; set; }

        public ModuleCapability(string name, string description, string category = "General", bool isEnabled = true)
        {
            Name = name;
            Description = description;
            Category = category;
            IsEnabled = isEnabled;
        }
    }

    /// <summary>
    /// Module validation result
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public List<string> Warnings { get; set; } = new List<string>();

        public static ValidationResult Success() => new ValidationResult { IsValid = true };
        
        public static ValidationResult Failure(params string[] errors) => new ValidationResult 
        { 
            IsValid = false, 
            Errors = new List<string>(errors) 
        };
    }
}
