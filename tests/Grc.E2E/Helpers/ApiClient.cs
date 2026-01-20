using RestSharp;
using Polly;
using System.Net;
using Newtonsoft.Json;

namespace Grc.E2E.Helpers
{
    public class ApiClient
    {
        private readonly RestClient _client;
        private readonly IAsyncPolicy<RestResponse> _retryPolicy;
        private string? _authToken;

        public ApiClient(string baseUrl = null)
        {
            baseUrl ??= TestConfig.ApiBaseUrl;
            _client = new RestClient(baseUrl);
            
            _retryPolicy = Policy
                .HandleResult<RestResponse>(r => !r.IsSuccessful && r.StatusCode != HttpStatusCode.BadRequest)
                .WaitAndRetryAsync(
                    TestConfig.ApiMaxRetries,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    onRetry: (outcome, timespan, retryCount, context) =>
                    {
                        Console.WriteLine($"Retry {retryCount} after {timespan} seconds");
                    });
        }

        public void SetAuthToken(string token)
        {
            _authToken = token;
            _client.AddDefaultHeader("Authorization", $"Bearer {token}");
        }

        public async Task<LoginResponse> LoginAsync(string email, string password)
        {
            var request = new RestRequest("/auth/login", Method.Post);
            request.AddJsonBody(new { email, password });
            
            var response = await _retryPolicy.ExecuteAsync(async () => 
                await _client.ExecuteAsync(request));

            if (!response.IsSuccessful)
                throw new Exception($"Login failed: {response.ErrorMessage ?? response.Content}");

            var loginResponse = JsonConvert.DeserializeObject<LoginResponse>(response.Content!);
            if (loginResponse?.Token != null)
            {
                SetAuthToken(loginResponse.Token);
            }
            
            return loginResponse!;
        }

        public async Task<TenantResponse> CreateTenantAsync(string name, string slug, string adminEmail)
        {
            var request = new RestRequest("/platform/admin/create-tenant", Method.Post);
            request.AddJsonBody(new
            {
                OrganizationName = name,
                TenantSlug = slug,
                AdminEmail = adminEmail,
                AdminFirstName = "Test",
                AdminLastName = "Admin"
            });

            var response = await _retryPolicy.ExecuteAsync(async () => 
                await _client.ExecuteAsync(request));

            if (!response.IsSuccessful)
                throw new Exception($"Create tenant failed: {response.ErrorMessage ?? response.Content}");

            return JsonConvert.DeserializeObject<TenantResponse>(response.Content!)!;
        }

        public async Task<InvitationResponse> SendInvitationAsync(string email, string role)
        {
            var request = new RestRequest("/invitations/send", Method.Post);
            request.AddJsonBody(new
            {
                Email = email,
                Role = role,
                Message = "You are invited to join our platform"
            });

            var response = await _retryPolicy.ExecuteAsync(async () => 
                await _client.ExecuteAsync(request));

            if (!response.IsSuccessful)
                throw new Exception($"Send invitation failed: {response.ErrorMessage ?? response.Content}");

            return JsonConvert.DeserializeObject<InvitationResponse>(response.Content!)!;
        }

        public async Task<bool> AcceptInvitationAsync(string invitationToken, string password)
        {
            var request = new RestRequest("/invitations/accept", Method.Post);
            request.AddJsonBody(new
            {
                Token = invitationToken,
                Password = password
            });

            var response = await _retryPolicy.ExecuteAsync(async () => 
                await _client.ExecuteAsync(request));

            return response.IsSuccessful;
        }

        public async Task<T> GetAsync<T>(string endpoint)
        {
            var request = new RestRequest(endpoint, Method.Get);
            
            var response = await _retryPolicy.ExecuteAsync(async () => 
                await _client.ExecuteAsync(request));

            if (!response.IsSuccessful)
                throw new Exception($"GET request failed: {response.ErrorMessage ?? response.Content}");

            return JsonConvert.DeserializeObject<T>(response.Content!)!;
        }

        public async Task<T> PostAsync<T>(string endpoint, object body)
        {
            var request = new RestRequest(endpoint, Method.Post);
            request.AddJsonBody(body);
            
            var response = await _retryPolicy.ExecuteAsync(async () => 
                await _client.ExecuteAsync(request));

            if (!response.IsSuccessful)
                throw new Exception($"POST request failed: {response.ErrorMessage ?? response.Content}");

            return JsonConvert.DeserializeObject<T>(response.Content!)!;
        }
    }

    public class LoginResponse
    {
        public string Token { get; set; }
        public string Email { get; set; }
        public List<string> Roles { get; set; }
    }

    public class TenantResponse
    {
        public bool Success { get; set; }
        public Guid TenantId { get; set; }
        public string TenantSlug { get; set; }
        public string AdminEmail { get; set; }
        public string TemporaryPassword { get; set; }
        public string Message { get; set; }
    }

    public class InvitationResponse
    {
        public string InvitationId { get; set; }
        public string Token { get; set; }
        public string Email { get; set; }
        public bool Success { get; set; }
    }
}
