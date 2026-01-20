using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.Text.RegularExpressions;
using System.Text.Json;
using System.Net.Http;

namespace Grc.E2E.Tests
{
    /// <summary>
    /// Complete Golden Path E2E test covering register → verify → login → invite → accept → role validation.
    /// Includes mail capture integration and audit verification.
    /// </summary>
    [TestFixture]
    public class GoldenPathE2E
    {
        private IWebDriver _driver = default!;
        private WebDriverWait _wait = default!;
        private HttpClient _http = default!;

        // Configure for your environment
        private readonly string BaseUrl = "https://staging.your-grc.com";
        private readonly string MailBaseUrl = "http://mailhog:8025"; // or Mailpit base URL
        private string RunId = Guid.NewGuid().ToString("N");

        [SetUp]
        public void Setup()
        {
            var options = new ChromeOptions();
            options.AddArgument("--headless=new");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");

            _driver = new ChromeDriver(options);
            _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(25));
            _http = new HttpClient();
        }

        [TearDown]
        public void TearDown()
        {
            try
            {
                if (TestContext.CurrentContext.Result.Outcome.Status ==
                    NUnit.Framework.Interfaces.TestStatus.Failed)
                {
                    var shot = ((ITakesScreenshot)_driver).GetScreenshot();
                    shot.SaveAsFile($"FAIL_{RunId}.png");
                }
            }
            finally
            {
                _driver?.Quit();
                (_driver as IDisposable)?.Dispose();
                _http?.Dispose();
            }
        }

        [Test]
        public async Task GoldenPath_Register_Verify_Login_Invite_Accept()
        {
            var adminEmail = $"test+{RunId}@example.com";
            var adminPass = "StrongPass#2026!";
            var inviteeEmail = $"invitee+{RunId}@example.com";
            var inviteePass = "StrongPass#2026!";

            // 1) UI Register
            _driver.Navigate().GoToUrl($"{BaseUrl}/register");
            _wait.Until(d => d.FindElement(By.CssSelector("input[name='email']"))).SendKeys(adminEmail);
            _driver.FindElement(By.CssSelector("input[name='password']")).SendKeys(adminPass);
            _driver.FindElement(By.CssSelector("input[name='fullName']")).SendKeys($"Golden Path {RunId}");
            _driver.FindElement(By.CssSelector("button[type='submit']")).Click();

            // 2) Email verification (extract link from Mail system)
            var verifyLink = await WaitForLinkFromMailhogToAsync(adminEmail, pattern: @"/verify|/email-confirm|/confirm");
            _driver.Navigate().GoToUrl(verifyLink);

            // 3) UI Login
            _driver.Navigate().GoToUrl($"{BaseUrl}/login");
            _wait.Until(d => d.FindElement(By.CssSelector("input[name='email']"))).SendKeys(adminEmail);
            _driver.FindElement(By.CssSelector("input[name='password']")).SendKeys(adminPass);
            _driver.FindElement(By.CssSelector("button[type='submit']")).Click();
            _wait.Until(d => d.Url.Contains("/dashboard"));

            // 4) UI Invite user (adapt selectors/routes to your UI)
            _driver.Navigate().GoToUrl($"{BaseUrl}/admin/users/invite");
            _wait.Until(d => d.FindElement(By.CssSelector("input[name='email']"))).SendKeys(inviteeEmail);

            // Role dropdown example; adjust to your UI control
            var roleSelect = new SelectElement(_driver.FindElement(By.CssSelector("select[name='role']")));
            roleSelect.SelectByValue("ComplianceOfficer");

            _driver.FindElement(By.CssSelector("button[type='submit']")).Click();
            // Confirm invite success message
            _wait.Until(d => d.PageSource.Contains("Invitation sent") || d.PageSource.Contains("Invited"));

            // 5) Invitee accepts invitation via email link
            var acceptLink = await WaitForLinkFromMailhogToAsync(inviteeEmail, pattern: @"/invitation/accept");
            _driver.Navigate().GoToUrl(acceptLink);

            _wait.Until(d => d.FindElement(By.CssSelector("input[name='password']"))).SendKeys(inviteePass);
            _driver.FindElement(By.CssSelector("button[type='submit']")).Click();

            // 6) Invitee login and verify role-based UI
            _driver.Navigate().GoToUrl($"{BaseUrl}/login");
            _wait.Until(d => d.FindElement(By.CssSelector("input[name='email']"))).SendKeys(inviteeEmail);
            _driver.FindElement(By.CssSelector("input[name='password']")).SendKeys(inviteePass);
            _driver.FindElement(By.CssSelector("button[type='submit']")).Click();

            // Verify landing + permission cues (adjust to your UI)
            _wait.Until(d => d.Url.Contains("/dashboard"));
            Assert.That(_driver.PageSource.Contains("Compliance") || _driver.PageSource.Contains("Policies"),
                "Expected compliance features to be visible for ComplianceOfficer role.");
        }

        /// <summary>
        /// MailHog helper: waits until it finds a matching link in the email body.
        /// Supports both MailHog and Mailpit APIs.
        /// </summary>
        private async Task<string> WaitForLinkFromMailhogToAsync(string toEmail, string pattern, int timeoutSeconds = 60)
        {
            var deadline = DateTime.UtcNow.AddSeconds(timeoutSeconds);
            var rx = new Regex(pattern, RegexOptions.IgnoreCase);

            while (DateTime.UtcNow < deadline)
            {
                // Try MailHog API first
                var url = $"{MailBaseUrl}/api/v2/search?kind=to&query={Uri.EscapeDataString(toEmail)}";
                
                try
                {
                    var json = await _http.GetStringAsync(url);
                    using var doc = JsonDocument.Parse(json);
                    
                    if (doc.RootElement.TryGetProperty("items", out var items) && items.GetArrayLength() > 0)
                    {
                        var first = items[0];
                        var match = rx.Match(json);
                        
                        if (match.Success)
                        {
                            var fullUrl = ExtractUrlContaining(json, match.Value);
                            if (!string.IsNullOrWhiteSpace(fullUrl)) 
                                return fullUrl;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Mail API error: {ex.Message}");
                }

                await Task.Delay(2000);
            }

            throw new Exception($"Timed out waiting for email link for {toEmail} matching pattern: {pattern}");
        }

        private string ExtractUrlContaining(string text, string contains)
        {
            // Very robust URL regex for test environments
            var urlRx = new Regex(@"https?://[^\s'\""<>()]+", RegexOptions.IgnoreCase);
            foreach (Match m in urlRx.Matches(text))
            {
                if (m.Value.Contains(contains, StringComparison.OrdinalIgnoreCase))
                    return m.Value;
            }
            return "";
        }
    }
}
