using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using FluentAssertions;
using Bogus;
using Grc.E2E.Pages;
using Grc.E2E.Helpers;

namespace Grc.E2E.Tests
{
    [TestFixture]
    public class GoldenPathTests
    {
        private IWebDriver _driver;
        private ApiClient _apiClient;
        private MailhogClient _mailhogClient;
        private Faker _faker;
        
        // Page objects
        private LoginPage _loginPage;
        private RegisterPage _registerPage;
        private TenantDashboardPage _dashboardPage;
        private InviteUserPage _inviteUserPage;
        private InvitationAcceptPage _invitationAcceptPage;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            _faker = new Faker();
            _apiClient = new ApiClient();
            _mailhogClient = new MailhogClient();
        }

        [SetUp]
        public void Setup()
        {
            // Initialize Chrome driver
            var options = new ChromeOptions();
            if (TestConfig.Headless)
            {
                options.AddArguments("--headless");
                options.AddArguments("--no-sandbox");
                options.AddArguments("--disable-dev-shm-usage");
            }
            
            _driver = new ChromeDriver(options);
            _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(TestConfig.ImplicitWaitSeconds);
            _driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(TestConfig.PageLoadTimeoutSeconds);
            _driver.Manage().Window.Maximize();
            
            // Initialize page objects
            _loginPage = new LoginPage(_driver);
            _registerPage = new RegisterPage(_driver);
            _dashboardPage = new TenantDashboardPage(_driver);
            _inviteUserPage = new InviteUserPage(_driver);
            _invitationAcceptPage = new InvitationAcceptPage(_driver);
            
            // Clear mailhog messages before each test
            _mailhogClient.DeleteAllMessagesAsync().Wait();
        }

        [TearDown]
        public void TearDown()
        {
            if (TestContext.CurrentContext.Result.Outcome.Status == NUnit.Framework.Interfaces.TestStatus.Failed)
            {
                if (TestConfig.TakeScreenshotOnFailure)
                {
                    TakeScreenshot();
                }
            }

            _driver?.Quit();
            (_driver as IDisposable)?.Dispose();
        }

        [Test, Order(1)]
        public void Test01_PlatformAdmin_Login()
        {
            // Arrange
            var (email, password) = TestConfig.TestAccounts.PlatformAdmin;
            
            // Act
            _loginPage.NavigateTo();
            _loginPage.Login(email, password);
            
            // Assert
            _loginPage.IsLoginSuccessful().Should().BeTrue("Platform admin should be able to login");
            _driver.Url.Should().NotContain("/Account/Login");
        }

        [Test, Order(2)]
        public async Task Test02_PlatformAdmin_CreateTenant()
        {
            // Arrange
            var (adminEmail, adminPassword) = TestConfig.TestAccounts.PlatformAdmin;
            await _apiClient.LoginAsync(adminEmail, adminPassword);
            
            var tenantName = _faker.Company.CompanyName();
            var tenantSlug = tenantName.ToLower().Replace(" ", "-").Replace(".", "");
            var tenantAdminEmail = _faker.Internet.Email();
            
            // Act
            var response = await _apiClient.CreateTenantAsync(tenantName, tenantSlug, tenantAdminEmail);
            
            // Assert
            response.Should().NotBeNull();
            response.Success.Should().BeTrue();
            response.TenantSlug.Should().Be(tenantSlug);
            response.AdminEmail.Should().Be(tenantAdminEmail);
            response.TemporaryPassword.Should().NotBeNullOrEmpty();
            
            // Verify email was sent
            var email = await _mailhogClient.GetLatestMessageForRecipientAsync(tenantAdminEmail);
            email.Should().NotBeNull("Welcome email should be sent to tenant admin");
            
            var tempPassword = _mailhogClient.ExtractTemporaryPassword(email);
            tempPassword.Should().NotBeNullOrEmpty();
        }

        [Test, Order(3)]
        public void Test03_NewUser_Registration()
        {
            // Arrange
            var email = _faker.Internet.Email();
            var password = "Test@Password123!";
            var firstName = _faker.Name.FirstName();
            var lastName = _faker.Name.LastName();
            var companyName = _faker.Company.CompanyName();
            
            // Act
            _registerPage.NavigateTo();
            _registerPage.FillRegistrationForm(email, password, firstName, lastName, companyName);
            _registerPage.ClickRegister();
            
            // Assert
            _registerPage.IsRegistrationSuccessful().Should().BeTrue("User should be able to register");
            
            // Verify we can login with the new account
            _loginPage.NavigateTo();
            _loginPage.Login(email, password);
            _loginPage.IsLoginSuccessful().Should().BeTrue("Newly registered user should be able to login");
        }

        [Test, Order(4)]
        public void Test04_TenantAdmin_Dashboard_Access()
        {
            // Arrange
            var (_, tenantSlug, adminEmail) = TestConfig.TestAccounts.DefaultTenant;
            // Note: In a real scenario, you'd need to create the tenant first or use a seeded one
            
            // Act
            _dashboardPage.NavigateTo(tenantSlug);
            
            // Assert
            _dashboardPage.IsOnDashboard().Should().BeTrue();
            _dashboardPage.GetWelcomeMessage().Should().NotBeNullOrEmpty();
            _dashboardPage.HasWidget("user-count-widget").Should().BeTrue();
            _dashboardPage.HasWidget("compliance-status-widget").Should().BeTrue();
        }

        [Test, Order(5)]
        public async Task Test05_InviteUser_AcceptInvitation_Flow()
        {
            // Arrange - Login as admin
            var (adminEmail, adminPassword) = TestConfig.TestAccounts.PlatformAdmin;
            await _apiClient.LoginAsync(adminEmail, adminPassword);
            
            // Create a new user to invite
            var inviteeEmail = _faker.Internet.Email();
            var inviteeFirstName = _faker.Name.FirstName();
            var inviteeLastName = _faker.Name.LastName();
            
            // Act 1: Send invitation via UI
            _loginPage.NavigateTo();
            _loginPage.Login(adminEmail, adminPassword);
            _inviteUserPage.NavigateTo();
            _inviteUserPage.SendInvitation(inviteeEmail, inviteeFirstName, inviteeLastName, "User", 
                "Welcome to our platform! Please accept this invitation to join our team.");
            
            // Assert invitation sent
            _inviteUserPage.IsInvitationSent().Should().BeTrue();
            
            // Act 2: Check invitation email
            var invitationEmail = await _mailhogClient.GetLatestMessageForRecipientAsync(inviteeEmail, 30);
            invitationEmail.Should().NotBeNull("Invitation email should be sent");
            
            var invitationToken = _mailhogClient.ExtractInvitationToken(invitationEmail);
            invitationToken.Should().NotBeNullOrEmpty("Invitation token should be in the email");
            
            // Act 3: Accept invitation
            _driver.Navigate().GoToUrl($"{TestConfig.BaseUrl}/Account/Logout"); // Logout first
            _invitationAcceptPage.NavigateTo(invitationToken);
            
            // Assert invitation is valid
            _invitationAcceptPage.IsInvitationValid().Should().BeTrue();
            _invitationAcceptPage.GetInvitationEmail().Should().Contain(inviteeEmail);
            
            // Act 4: Complete acceptance
            var newPassword = "Accepted@Password123!";
            _invitationAcceptPage.AcceptInvitation(newPassword, inviteeFirstName, inviteeLastName, _faker.Phone.PhoneNumber());
            
            // Assert acceptance successful
            _invitationAcceptPage.IsAcceptanceSuccessful().Should().BeTrue();
            
            // Act 5: Verify new user can login
            _loginPage.NavigateTo();
            _loginPage.Login(inviteeEmail, newPassword);
            
            // Assert login successful
            _loginPage.IsLoginSuccessful().Should().BeTrue("Invited user should be able to login after accepting invitation");
        }

        [Test, Order(6)]
        public void Test06_UserManagement_ViewInvitationHistory()
        {
            // Arrange
            var (adminEmail, adminPassword) = TestConfig.TestAccounts.PlatformAdmin;
            
            // Act
            _loginPage.NavigateTo();
            _loginPage.Login(adminEmail, adminPassword);
            _inviteUserPage.NavigateTo();
            
            var history = _inviteUserPage.GetInvitationHistory();
            
            // Assert
            history.Should().NotBeNull();
            history.Should().HaveCountGreaterThan(0, "There should be invitation history from previous tests");
            
            var pendingInvitations = history.Where(i => i.Status == "Pending").ToList();
            var acceptedInvitations = history.Where(i => i.Status == "Accepted").ToList();
            
            // Test resend functionality for pending invitations
            if (pendingInvitations.Any())
            {
                var firstPending = pendingInvitations.First();
                var resent = _inviteUserPage.ResendInvitation(firstPending.Email);
                resent.Should().BeTrue("Should be able to resend pending invitations");
            }
        }

        [Test, Order(7)]
        public void Test07_BulkInvite_CSV_Upload()
        {
            // Arrange
            var (adminEmail, adminPassword) = TestConfig.TestAccounts.PlatformAdmin;
            var csvPath = CreateTestCsvFile();
            
            // Act
            _loginPage.NavigateTo();
            _loginPage.Login(adminEmail, adminPassword);
            _inviteUserPage.NavigateTo();
            _inviteUserPage.SwitchToBulkInvite();
            _inviteUserPage.UploadCsvFile(csvPath);
            _inviteUserPage.ClickUploadAndInvite();
            
            // Assert
            _inviteUserPage.IsInvitationSent().Should().BeTrue("Bulk invite should succeed");
            _inviteUserPage.GetSuccessMessage().Should().Contain("invitations sent successfully");
            
            // Cleanup
            File.Delete(csvPath);
        }

        [Test, Order(8)]
        public void Test08_Logout_RedirectToLogin()
        {
            // Arrange
            var (adminEmail, adminPassword) = TestConfig.TestAccounts.PlatformAdmin;
            
            // Act
            _loginPage.NavigateTo();
            _loginPage.Login(adminEmail, adminPassword);
            _dashboardPage.NavigateTo();
            _dashboardPage.Logout();
            
            // Assert
            _driver.Url.Should().Contain("/Account/Login", "User should be redirected to login page after logout");
        }

        [Test, Order(9)]
        public void Test09_InvalidLogin_ShowsError()
        {
            // Arrange
            var invalidEmail = "invalid@example.com";
            var invalidPassword = "WrongPassword123!";
            
            // Act
            _loginPage.NavigateTo();
            _loginPage.Login(invalidEmail, invalidPassword);
            
            // Assert
            _loginPage.IsLoginSuccessful().Should().BeFalse();
            _loginPage.IsOnLoginPage().Should().BeTrue();
            _loginPage.GetErrorMessage().Should().NotBeNullOrEmpty();
            _loginPage.GetErrorMessage().Should().Contain("Invalid login attempt");
        }

        [Test, Order(10)]
        public void Test10_PasswordReset_Flow()
        {
            // Arrange
            var userEmail = _faker.Internet.Email();
            // First create a user (simplified for demo - in real test, use API or previous test data)
            
            // Act
            _loginPage.NavigateTo();
            _loginPage.ClickForgotPassword();
            
            // Assert
            _driver.Url.Should().Contain("/Account/ForgotPassword");
            // Continue with password reset flow testing...
        }

        private void TakeScreenshot()
        {
            try
            {
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                var testName = TestContext.CurrentContext.Test.Name;
                var fileName = $"{testName}_{timestamp}.png";
                
                var screenshotPath = Path.Combine(TestConfig.ScreenshotPath, fileName);
                Directory.CreateDirectory(TestConfig.ScreenshotPath);
                
                var screenshot = ((ITakesScreenshot)_driver).GetScreenshot();
                screenshot.SaveAsFile(screenshotPath);
                
                TestContext.WriteLine($"Screenshot saved: {screenshotPath}");
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"Failed to take screenshot: {ex.Message}");
            }
        }

        private string CreateTestCsvFile()
        {
            var csvContent = "Email,FirstName,LastName,Role\n";
            for (int i = 0; i < 3; i++)
            {
                csvContent += $"{_faker.Internet.Email()},{_faker.Name.FirstName()},{_faker.Name.LastName()},User\n";
            }
            
            var csvPath = Path.Combine(Path.GetTempPath(), $"bulk_invite_{Guid.NewGuid()}.csv");
            File.WriteAllText(csvPath, csvContent);
            return csvPath;
        }
    }
}
