using System.Collections.Generic;
using System.Threading.Tasks;

namespace GrcMvc.Services.Interfaces
{
    /// <summary>
    /// Service for discovering and managing API endpoints
    /// </summary>
    public interface IEndpointDiscoveryService
    {
        /// <summary>
        /// Get all API endpoints in the application
        /// </summary>
        Task<List<EndpointInfo>> GetAllEndpointsAsync();

        /// <summary>
        /// Get endpoints by controller
        /// </summary>
        Task<List<EndpointInfo>> GetEndpointsByControllerAsync(string controllerName);

        /// <summary>
        /// Get endpoints by HTTP method
        /// </summary>
        Task<List<EndpointInfo>> GetEndpointsByMethodAsync(string httpMethod);

        /// <summary>
        /// Get endpoint statistics
        /// </summary>
        Task<EndpointStatistics> GetStatisticsAsync();
    }

    /// <summary>
    /// Information about an API endpoint
    /// </summary>
    public class EndpointInfo
    {
        public string Route { get; set; } = string.Empty;
        public string HttpMethod { get; set; } = string.Empty;
        public string ControllerName { get; set; } = string.Empty;
        public string ActionName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool RequiresAuth { get; set; }
        public List<string> RequiredPermissions { get; set; } = new();
        public string? Policy { get; set; }
        public bool IsProductionReady { get; set; } = true;
    }

    /// <summary>
    /// Endpoint statistics
    /// </summary>
    public class EndpointStatistics
    {
        public int TotalEndpoints { get; set; }
        public int GetEndpoints { get; set; }
        public int PostEndpoints { get; set; }
        public int PutEndpoints { get; set; }
        public int DeleteEndpoints { get; set; }
        public int PatchEndpoints { get; set; }
        public int AuthenticatedEndpoints { get; set; }
        public int PublicEndpoints { get; set; }
        public int Controllers { get; set; }
    }
}
