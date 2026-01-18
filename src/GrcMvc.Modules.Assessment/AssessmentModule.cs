using GrcMvc.PluginFramework;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace GrcMvc.Modules.Assessment
{
    /// <summary>
    /// Assessment Management Module
    /// </summary>
    public class AssessmentModule : ModuleBase
    {
        public override string Id => "GrcMvc.Modules.Assessment";
        public override string Name => "Assessment Management";
        public override string Version => "1.0.0";
        public override string Description => "Comprehensive assessment management including templates, execution, and reporting";
        public override string Author => "GRC Team";
        public override ModulePriority Priority => ModulePriority.High;

        public override void ConfigureServices(IServiceCollection services)
        {
            // Register assessment services
            services.AddScoped<IAssessmentService, AssessmentService>();
            services.AddScoped<IAssessmentExecutionService, AssessmentExecutionService>();
            services.AddScoped<IAssessmentTemplateService, AssessmentTemplateService>();
            services.AddScoped<IAssessmentReportingService, AssessmentReportingService>();
            
            // Register repositories
            services.AddScoped<IAssessmentRepository, AssessmentRepository>();
            
            // Register validators
            services.AddScoped<IValidator<Assessment>, AssessmentValidator>();
            
            // Register event handlers
            services.AddScoped<IEventHandler<AssessmentCreatedEvent>, AssessmentCreatedEventHandler>();
            services.AddScoped<IEventHandler<AssessmentCompletedEvent>, AssessmentCompletedEventHandler>();
            
            Logger?.LogInformation($"Configured services for {Name} module");
        }

        public override void Configure(IApplicationBuilder app)
        {
            // Configure module-specific middleware if needed
            app.UseMiddleware<AssessmentAuthorizationMiddleware>();
            
            Logger?.LogInformation($"Configured middleware for {Name} module");
        }

        public override void ConfigureDatabase(ModelBuilder modelBuilder)
        {
            // Configure assessment entities
            modelBuilder.Entity<Assessment>(entity =>
            {
                entity.ToTable("Assessments", "assessment");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.ControlId);
                entity.HasIndex(e => e.Status);
                entity.HasIndex(e => e.DueDate);
                
                entity.HasOne(e => e.Control)
                    .WithMany(c => c.Assessments)
                    .HasForeignKey(e => e.ControlId);
                    
                entity.HasOne(e => e.AssignedTo)
                    .WithMany()
                    .HasForeignKey(e => e.AssignedToId);
            });
            
            modelBuilder.Entity<AssessmentTemplate>(entity =>
            {
                entity.ToTable("AssessmentTemplates", "assessment");
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Name);
                
                entity.HasMany(e => e.Questions)
                    .WithOne(q => q.Template)
                    .HasForeignKey(q => q.TemplateId);
            });
            
            modelBuilder.Entity<AssessmentResponse>(entity =>
            {
                entity.ToTable("AssessmentResponses", "assessment");
                entity.HasKey(e => e.Id);
                
                entity.HasOne(e => e.Assessment)
                    .WithMany(a => a.Responses)
                    .HasForeignKey(e => e.AssessmentId);
            });
            
            Logger?.LogInformation($"Configured database schema for {Name} module");
        }

        public override IEnumerable<ModuleDependency> GetDependencies()
        {
            return new[]
            {
                new ModuleDependency("GrcMvc.Core", "1.0.0"),
                new ModuleDependency("GrcMvc.Modules.Control", "1.0.0"),
                new ModuleDependency("GrcMvc.Modules.Workflow", "1.0.0", isRequired: false)
            };
        }

        public override IEnumerable<ModuleCapability> GetCapabilities()
        {
            return new[]
            {
                new ModuleCapability("Assessment Creation", "Create and manage assessments", "Core"),
                new ModuleCapability("Assessment Templates", "Design reusable assessment templates", "Templates"),
                new ModuleCapability("Assessment Execution", "Execute assessments and collect responses", "Execution"),
                new ModuleCapability("Assessment Reporting", "Generate assessment reports and analytics", "Reporting"),
                new ModuleCapability("Bulk Operations", "Perform bulk assessment operations", "Advanced"),
                new ModuleCapability("Assessment Workflows", "Automated assessment workflows", "Workflow", isEnabled: false)
            };
        }

        public override ValidationResult ValidateConfiguration()
        {
            var errors = new List<string>();
            
            // Validate required configuration
            // Example: Check if required settings exist
            
            if (errors.Any())
            {
                return ValidationResult.Failure(errors.ToArray());
            }
            
            return ValidationResult.Success();
        }
    }
}
