using Microsoft.Extensions.Configuration;
using Volo.Abp.Settings;
using Volo.Abp.DependencyInjection;

namespace GrcMvc.Settings
{
    public class ConnectionStringSettingValueProvider : ISettingValueProvider, ITransientDependency
    {
        public const string ProviderName = "ConnectionString";

        private readonly IConfiguration _configuration;

        public ConnectionStringSettingValueProvider(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string Name => ProviderName;

        public async Task<string?> GetOrNullAsync(SettingDefinition setting)
        {
            // Map connection string settings to actual connection strings
            // This is called by ABP Settings system - reads from IConfiguration dynamically
            var value = setting.Name switch
            {
                AppSettings.DefaultConnection => _configuration.GetConnectionString("DefaultConnection"),
                AppSettings.GrcAuthDb => _configuration.GetConnectionString("GrcAuthDb"),
                AppSettings.HangfireConnection => _configuration.GetConnectionString("HangfireConnection"),
                AppSettings.Redis => _configuration.GetConnectionString("Redis"),
                _ => null
            };

            // Debug logging
            if (!string.IsNullOrEmpty(value))
            {
                Console.WriteLine("[ABP-SETTINGS] ✅ ConnectionString provider: {Setting} = {Value}", 
                    setting.Name, MaskConnectionString(value));
            }
            else
            {
                Console.WriteLine("[ABP-SETTINGS] ❌ ConnectionString provider: {Setting} = (not found)", setting.Name);
            }

            return await Task.FromResult(value);
        }

        private static string MaskConnectionString(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
                return "[empty]";
            
            try
            {
                var parts = connectionString.Split(';');
                var masked = new List<string>();
                foreach (var part in parts)
                {
                    if (part.StartsWith("Password=", StringComparison.OrdinalIgnoreCase) ||
                        part.StartsWith("Pwd=", StringComparison.OrdinalIgnoreCase))
                    {
                        masked.Add(part.Split('=')[0] + "=***");
                    }
                    else
                    {
                        masked.Add(part);
                    }
                }
                return string.Join(";", masked);
            }
            catch
            {
                return "[invalid format]";
            }
        }

        public Task<List<SettingValue>> GetAllAsync(SettingDefinition[] settings)
        {
            var values = new List<SettingValue>();

            foreach (var setting in settings)
            {
                var value = GetOrNullAsync(setting).Result;
                if (value != null)
                {
                    values.Add(new SettingValue(setting.Name, value));
                }
            }

            return Task.FromResult(values);
        }
    }
}
