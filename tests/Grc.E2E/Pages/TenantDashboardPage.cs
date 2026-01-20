using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using Grc.E2E.Helpers;

namespace Grc.E2E.Pages
{
    public class TenantDashboardPage
    {
        private readonly IWebDriver _driver;
        private readonly WebDriverWait _wait;
        
        // Navigation elements
        private IWebElement DashboardLink => _driver.FindElement(By.LinkText("Dashboard"));
        private IWebElement UsersLink => _driver.FindElement(By.LinkText("Users"));
        private IWebElement InviteUsersLink => _driver.FindElement(By.LinkText("Invite Users"));
        private IWebElement SettingsLink => _driver.FindElement(By.LinkText("Settings"));
        private IWebElement ProfileDropdown => _driver.FindElement(By.ClassName("user-profile-dropdown"));
        private IWebElement LogoutLink => _driver.FindElement(By.LinkText("Logout"));
        
        // Dashboard widgets
        private IWebElement WelcomeMessage => _driver.FindElement(By.ClassName("welcome-message"));
        private IWebElement UserCountWidget => _driver.FindElement(By.Id("user-count-widget"));
        private IWebElement ActivityWidget => _driver.FindElement(By.Id("activity-widget"));
        private IWebElement ComplianceStatusWidget => _driver.FindElement(By.Id("compliance-status-widget"));
        
        // Quick actions
        private IWebElement InviteUserButton => _driver.FindElement(By.XPath("//button[contains(text(), 'Invite User')]"));
        private IWebElement CreateAssessmentButton => _driver.FindElement(By.XPath("//button[contains(text(), 'New Assessment')]"));
        private IWebElement ViewReportsButton => _driver.FindElement(By.XPath("//button[contains(text(), 'View Reports')]"));
        
        public TenantDashboardPage(IWebDriver driver)
        {
            _driver = driver;
            _wait = new WebDriverWait(driver, TimeSpan.FromSeconds(TestConfig.ImplicitWaitSeconds));
        }

        public void NavigateTo(string tenantSlug = null)
        {
            var url = string.IsNullOrEmpty(tenantSlug) 
                ? $"{TestConfig.BaseUrl}/Dashboard"
                : $"{TestConfig.BaseUrl}/t/{tenantSlug}/Dashboard";
                
            _driver.Navigate().GoToUrl(url);
            WaitForPageLoad();
        }

        public bool IsOnDashboard()
        {
            return _driver.Url.Contains("/Dashboard");
        }

        public string GetWelcomeMessage()
        {
            try
            {
                _wait.Until(ExpectedConditions.ElementIsVisible(By.ClassName("welcome-message")));
                return WelcomeMessage.Text;
            }
            catch
            {
                return string.Empty;
            }
        }

        public int GetUserCount()
        {
            try
            {
                _wait.Until(ExpectedConditions.ElementIsVisible(By.Id("user-count-widget")));
                var countText = UserCountWidget.FindElement(By.ClassName("widget-value")).Text;
                return int.TryParse(countText, out var count) ? count : 0;
            }
            catch
            {
                return 0;
            }
        }

        public void NavigateToUsers()
        {
            UsersLink.Click();
            WaitForPageLoad();
        }

        public void NavigateToInviteUsers()
        {
            InviteUsersLink.Click();
            WaitForPageLoad();
        }

        public void NavigateToSettings()
        {
            SettingsLink.Click();
            WaitForPageLoad();
        }

        public void ClickInviteUserQuickAction()
        {
            InviteUserButton.Click();
        }

        public void ClickCreateAssessment()
        {
            CreateAssessmentButton.Click();
        }

        public void ClickViewReports()
        {
            ViewReportsButton.Click();
        }

        public void Logout()
        {
            try
            {
                // Try to find and click profile dropdown first
                ProfileDropdown.Click();
                Thread.Sleep(500); // Wait for dropdown to expand
                LogoutLink.Click();
            }
            catch
            {
                // If dropdown doesn't exist, try direct logout link
                try
                {
                    LogoutLink.Click();
                }
                catch
                {
                    // Navigate directly to logout URL as fallback
                    _driver.Navigate().GoToUrl($"{TestConfig.BaseUrl}/Account/Logout");
                }
            }
        }

        public bool HasWidget(string widgetId)
        {
            return _driver.FindElements(By.Id(widgetId)).Count > 0;
        }

        public string GetComplianceStatus()
        {
            try
            {
                _wait.Until(ExpectedConditions.ElementIsVisible(By.Id("compliance-status-widget")));
                return ComplianceStatusWidget.FindElement(By.ClassName("status-value")).Text;
            }
            catch
            {
                return "Unknown";
            }
        }

        public List<string> GetRecentActivities()
        {
            try
            {
                _wait.Until(ExpectedConditions.ElementIsVisible(By.Id("activity-widget")));
                var activities = ActivityWidget.FindElements(By.ClassName("activity-item"));
                return activities.Select(a => a.Text).ToList();
            }
            catch
            {
                return new List<string>();
            }
        }

        private void WaitForPageLoad()
        {
            _wait.Until(driver => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));
        }
    }
}
