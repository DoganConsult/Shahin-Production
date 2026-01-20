using RestSharp;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace Grc.E2E.Helpers
{
    public class MailhogClient
    {
        private readonly RestClient _client;

        public MailhogClient(string baseUrl = null)
        {
            baseUrl ??= TestConfig.MailhogApiUrl;
            _client = new RestClient(baseUrl);
        }

        public async Task<List<MailhogMessage>> GetMessagesAsync(int limit = 50)
        {
            var request = new RestRequest($"/messages?limit={limit}", Method.Get);
            var response = await _client.ExecuteAsync(request);

            if (!response.IsSuccessful)
                throw new Exception($"Failed to get messages from Mailhog: {response.ErrorMessage}");

            var result = JsonConvert.DeserializeObject<MailhogResponse>(response.Content!);
            return result?.Items ?? new List<MailhogMessage>();
        }

        public async Task<MailhogMessage?> GetLatestMessageForRecipientAsync(string email, int timeoutSeconds = 30)
        {
            var startTime = DateTime.UtcNow;
            
            while ((DateTime.UtcNow - startTime).TotalSeconds < timeoutSeconds)
            {
                var messages = await GetMessagesAsync();
                var message = messages.FirstOrDefault(m => 
                    m.Content?.Headers?.To?.Any(to => to.Contains(email, StringComparison.OrdinalIgnoreCase)) == true);
                
                if (message != null)
                    return message;
                
                await Task.Delay(1000);
            }
            
            return null;
        }

        public async Task DeleteAllMessagesAsync()
        {
            var request = new RestRequest("/messages", Method.Delete);
            await _client.ExecuteAsync(request);
        }

        public string? ExtractInvitationToken(MailhogMessage message)
        {
            if (message?.Content?.Body == null)
                return null;

            // Look for invitation token in the email body
            // Pattern might be: /invitation/accept?token=xxx or similar
            var pattern = @"token=([a-zA-Z0-9\-_]+)";
            var match = Regex.Match(message.Content.Body, pattern);
            
            if (match.Success)
                return match.Groups[1].Value;

            // Alternative pattern for links
            pattern = @"/invitation/accept/([a-zA-Z0-9\-_]+)";
            match = Regex.Match(message.Content.Body, pattern);
            
            if (match.Success)
                return match.Groups[1].Value;

            return null;
        }

        public string? ExtractPasswordResetToken(MailhogMessage message)
        {
            if (message?.Content?.Body == null)
                return null;

            // Look for password reset token
            var pattern = @"reset[_-]?token=([a-zA-Z0-9\-_]+)";
            var match = Regex.Match(message.Content.Body, pattern, RegexOptions.IgnoreCase);
            
            if (match.Success)
                return match.Groups[1].Value;

            // Alternative pattern
            pattern = @"/password/reset/([a-zA-Z0-9\-_]+)";
            match = Regex.Match(message.Content.Body, pattern);
            
            if (match.Success)
                return match.Groups[1].Value;

            return null;
        }

        public string? ExtractTemporaryPassword(MailhogMessage message)
        {
            if (message?.Content?.Body == null)
                return null;

            // Look for temporary password patterns
            var patterns = new[]
            {
                @"(?:Temporary Password|Password|Your password):\s*([A-Za-z0-9!@#$%^&*]+)",
                @"password is:\s*([A-Za-z0-9!@#$%^&*]+)",
                @"Admin@[A-Za-z0-9]+!20\d{2}"
            };

            foreach (var pattern in patterns)
            {
                var match = Regex.Match(message.Content.Body, pattern, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    return match.Groups.Count > 1 ? match.Groups[1].Value : match.Value;
                }
            }

            return null;
        }
    }

    public class MailhogResponse
    {
        public int Total { get; set; }
        public int Count { get; set; }
        public int Start { get; set; }
        public List<MailhogMessage> Items { get; set; }
    }

    public class MailhogMessage
    {
        public string ID { get; set; }
        public MailhogFrom From { get; set; }
        public List<MailhogTo> To { get; set; }
        public MailhogContent Content { get; set; }
        public DateTime Created { get; set; }
    }

    public class MailhogFrom
    {
        public string Mailbox { get; set; }
        public string Domain { get; set; }
    }

    public class MailhogTo
    {
        public string Mailbox { get; set; }
        public string Domain { get; set; }
    }

    public class MailhogContent
    {
        public MailhogHeaders Headers { get; set; }
        public string Body { get; set; }
        public int Size { get; set; }
        public string MIME { get; set; }
    }

    public class MailhogHeaders
    {
        public List<string> From { get; set; }
        public List<string> To { get; set; }
        public List<string> Subject { get; set; }
        [JsonProperty("Content-Type")]
        public List<string> ContentType { get; set; }
    }
}
