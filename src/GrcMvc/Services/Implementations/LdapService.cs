using Novell.Directory.Ldap;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using GrcMvc.Services.Interfaces;
using System.Text;

namespace GrcMvc.Services.Implementations;

/// <summary>
/// LDAP / Active Directory Service Implementation
/// Handles authentication and user lookup via LDAP/AD
/// </summary>
public class LdapService : ILdapService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<LdapService> _logger;
    private readonly string _server;
    private readonly int _port;
    private readonly string _baseDn;
    private readonly bool _useSsl;

    public LdapService(
        IConfiguration configuration,
        ILogger<LdapService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        _server = _configuration["Ldap:Server"] ?? "localhost";
        _port = _configuration.GetValue<int>("Ldap:Port", 389);
        _baseDn = _configuration["Ldap:BaseDn"] ?? "";
        _useSsl = _configuration.GetValue<bool>("Ldap:UseSsl", false);
    }

    public bool IsEnabled => _configuration.GetValue<bool>("Ldap:Enabled", false);

    public async Task<LdapUser?> AuthenticateAsync(string username, string password)
    {
        if (!IsEnabled)
        {
            _logger.LogWarning("LDAP authentication attempted but LDAP is not enabled");
            return null;
        }

        try
        {
            using var connection = new LdapConnection();
            
            if (_useSsl)
            {
                connection.SecureSocketLayer = true;
            }

            await Task.Run(() => connection.Connect(_server, _port));

            // Build user DN (Distinguished Name)
            var userDn = BuildUserDn(username);

            // Attempt to bind (authenticate) with user credentials
            await Task.Run(() => connection.Bind(userDn, password));

            if (connection.Bound)
            {
                // Authentication successful, retrieve user attributes
                var user = await GetUserAttributesAsync(connection, userDn, username);
                _logger.LogInformation("LDAP authentication successful for user: {Username}", username);
                return user;
            }

            return null;
        }
        catch (LdapException ex)
        {
            _logger.LogWarning(ex, "LDAP authentication failed for user: {Username}, Error: {ErrorCode}", username, ex.ResultCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during LDAP authentication for user: {Username}", username);
            return null;
        }
    }

    public async Task<LdapUser?> FindUserAsync(string username)
    {
        if (!IsEnabled)
        {
            return null;
        }

        try
        {
            using var connection = new LdapConnection();
            
            if (_useSsl)
            {
                connection.SecureSocketLayer = true;
            }

            await Task.Run(() => connection.Connect(_server, _port));

            // Bind with service account for search
            var serviceAccountDn = _configuration["Ldap:ServiceAccountDn"] ?? "";
            var serviceAccountPassword = _configuration["Ldap:ServiceAccountPassword"] ?? "";
            
            if (!string.IsNullOrEmpty(serviceAccountDn))
            {
                await Task.Run(() => connection.Bind(serviceAccountDn, serviceAccountPassword));
            }
            else
            {
                // Anonymous bind (if allowed)
                await Task.Run(() => connection.Bind(null, null));
            }

            // Search for user
            var searchFilter = _configuration["Ldap:UserSearchFilter"] ?? $"(sAMAccountName={username})";
            var attributes = new[] { "sAMAccountName", "mail", "displayName", "givenName", "sn", "memberOf", "distinguishedName" };

            var searchResults = await Task.Run(() => 
                connection.Search(_baseDn, LdapConnection.ScopeSub, searchFilter, attributes, false));

            if (searchResults.HasMore())
            {
                var entry = searchResults.Next();
                var user = MapLdapEntryToUser(entry, username);
                return user;
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding LDAP user: {Username}", username);
            return null;
        }
    }

    public async Task<List<LdapUser>> SearchUsersAsync(string searchTerm, int maxResults = 100)
    {
        if (!IsEnabled)
        {
            return new List<LdapUser>();
        }

        var users = new List<LdapUser>();

        try
        {
            using var connection = new LdapConnection();
            
            if (_useSsl)
            {
                connection.SecureSocketLayer = true;
            }

            await Task.Run(() => connection.Connect(_server, _port));

            // Bind with service account
            var serviceAccountDn = _configuration["Ldap:ServiceAccountDn"] ?? "";
            var serviceAccountPassword = _configuration["Ldap:ServiceAccountPassword"] ?? "";
            
            if (!string.IsNullOrEmpty(serviceAccountDn))
            {
                await Task.Run(() => connection.Bind(serviceAccountDn, serviceAccountPassword));
            }

            // Build search filter
            var searchFilter = $"(&(|(sAMAccountName=*{searchTerm}*)(displayName=*{searchTerm}*)(mail=*{searchTerm}*)))";
            var attributes = new[] { "sAMAccountName", "mail", "displayName", "givenName", "sn", "memberOf" };

            var searchResults = await Task.Run(() => 
                connection.Search(_baseDn, LdapConnection.ScopeSub, searchFilter, attributes, false));

            int count = 0;
            while (searchResults.HasMore() && count < maxResults)
            {
                var entry = searchResults.Next();
                var user = MapLdapEntryToUser(entry, null);
                if (user != null)
                {
                    users.Add(user);
                    count++;
                }
            }

            _logger.LogInformation("LDAP search found {Count} users for term: {SearchTerm}", users.Count, searchTerm);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching LDAP users: {SearchTerm}", searchTerm);
        }

        return users;
    }

    private string BuildUserDn(string username)
    {
        var userDnFormat = _configuration["Ldap:UserDnFormat"] ?? "{username}@{domain}";
        var domain = _configuration["Ldap:Domain"] ?? "";
        
        if (userDnFormat.Contains("{username}"))
        {
            return userDnFormat.Replace("{username}", username).Replace("{domain}", domain);
        }

        // Default: assume username is already a DN or use base DN
        if (username.Contains("="))
        {
            return username; // Already a DN
        }

        // Build DN from base DN and username
        var userAttribute = _configuration["Ldap:UserAttribute"] ?? "sAMAccountName";
        return $"{userAttribute}={username},{_baseDn}";
    }

    private async Task<LdapUser?> GetUserAttributesAsync(LdapConnection connection, string userDn, string username)
    {
        try
        {
            var attributes = new[] { "sAMAccountName", "mail", "displayName", "givenName", "sn", "memberOf", "distinguishedName" };
            var searchResults = await Task.Run(() => 
                connection.Search(userDn, LdapConnection.ScopeBase, "(objectclass=*)", attributes, false));

            if (searchResults.HasMore())
            {
                var entry = searchResults.Next();
                return MapLdapEntryToUser(entry, username);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error retrieving user attributes for: {UserDn}", userDn);
        }

        // Fallback: return basic user info
        return new LdapUser
        {
            Username = username,
            DistinguishedName = userDn
        };
    }

    private LdapUser? MapLdapEntryToUser(LdapEntry entry, string? username)
    {
        try
        {
            var user = new LdapUser
            {
                Username = GetAttributeValue(entry, "sAMAccountName") ?? username ?? "",
                Email = GetAttributeValue(entry, "mail") ?? "",
                DisplayName = GetAttributeValue(entry, "displayName") ?? "",
                FirstName = GetAttributeValue(entry, "givenName") ?? "",
                LastName = GetAttributeValue(entry, "sn") ?? "",
                DistinguishedName = GetAttributeValue(entry, "distinguishedName") ?? ""
            };

            // Get group memberships
            var memberOf = entry.GetAttribute("memberOf");
            if (memberOf != null)
            {
                user.Groups = new List<string>();
                foreach (var groupDn in memberOf.StringValueArray)
                {
                    // Extract CN from DN (e.g., "CN=GroupName,OU=Groups,DC=domain,DC=com" -> "GroupName")
                    var cnMatch = System.Text.RegularExpressions.Regex.Match(groupDn, @"CN=([^,]+)");
                    if (cnMatch.Success)
                    {
                        user.Groups.Add(cnMatch.Groups[1].Value);
                    }
                }
            }

            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error mapping LDAP entry to user");
            return null;
        }
    }

    private string? GetAttributeValue(LdapEntry entry, string attributeName)
    {
        try
        {
            var attribute = entry.GetAttribute(attributeName);
            return attribute?.StringValue;
        }
        catch
        {
            return null;
        }
    }
}

/// <summary>
/// LDAP Service Interface
/// </summary>
public interface ILdapService
{
    bool IsEnabled { get; }
    Task<LdapUser?> AuthenticateAsync(string username, string password);
    Task<LdapUser?> FindUserAsync(string username);
    Task<List<LdapUser>> SearchUsersAsync(string searchTerm, int maxResults = 100);
}

/// <summary>
/// LDAP User Model
/// </summary>
public class LdapUser
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string DistinguishedName { get; set; } = string.Empty;
    public List<string> Groups { get; set; } = new();
}
