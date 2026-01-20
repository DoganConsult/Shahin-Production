using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using Grc.E2E.Helpers;

namespace Grc.E2E.Pages
{
    public class LoginPage
    {
        private readonly IWebDriver _driver;
        private readonly WebDriverWait _wait;
        
        // Page elements
        private IWebElement EmailInput => _driver.FindElement(By.Id("Input_Email"));
        private IWebElement PasswordInput => _driver.FindElement(By.Id("Input_Password"));
        private IWebElement RememberMeCheckbox => _driver.FindElement(By.Id("Input_RememberMe"));
        private IWebElement LoginButton => _driver.FindElement(By.XPath("//button[contains(@class, 'btn-primary') and contains(text(), 'Log in')]"));
        private IWebElement RegisterLink => _driver.FindElement(By.LinkText("Register"));
        private IWebElement ForgotPasswordLink => _driver.FindElement(By.LinkText("Forgot your password?"));
        
        // Error messages
        private IWebElement ValidationSummary => _driver.FindElement(By.ClassName("validation-summary-errors"));
        private bool HasValidationErrors => _driver.FindElements(By.ClassName("validation-summary-errors")).Count > 0;
        
        public LoginPage(IWebDriver driver)
        {
            _driver = driver;
            _wait = new WebDriverWait(driver, TimeSpan.FromSeconds(TestConfig.ImplicitWaitSeconds));
        }

        public void NavigateTo()
        {
            _driver.Navigate().GoToUrl($"{TestConfig.BaseUrl}/Account/Login");
            WaitForPageLoad();
        }

        public void Login(string email, string password, bool rememberMe = false)
        {
            EnterEmail(email);
            EnterPassword(password);
            
            if (rememberMe)
                CheckRememberMe();
            
            ClickLogin();
        }

        public void EnterEmail(string email)
        {
            _wait.Until(ExpectedConditions.ElementIsVisible(By.Id("Input_Email")));
            EmailInput.Clear();
            EmailInput.SendKeys(email);
        }

        public void EnterPassword(string password)
        {
            _wait.Until(ExpectedConditions.ElementIsVisible(By.Id("Input_Password")));
            PasswordInput.Clear();
            PasswordInput.SendKeys(password);
        }

        public void CheckRememberMe()
        {
            if (!RememberMeCheckbox.Selected)
                RememberMeCheckbox.Click();
        }

        public void ClickLogin()
        {
            LoginButton.Click();
        }

        public void ClickRegister()
        {
            RegisterLink.Click();
        }

        public void ClickForgotPassword()
        {
            ForgotPasswordLink.Click();
        }

        public bool IsLoginSuccessful()
        {
            try
            {
                // Wait for redirect after successful login
                _wait.Until(driver => !driver.Url.Contains("/Account/Login"));
                return true;
            }
            catch
            {
                return false;
            }
        }

        public string GetErrorMessage()
        {
            if (HasValidationErrors)
                return ValidationSummary.Text;
            
            return string.Empty;
        }

        public bool IsOnLoginPage()
        {
            return _driver.Url.Contains("/Account/Login");
        }

        private void WaitForPageLoad()
        {
            _wait.Until(driver => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));
        }
    }
}
