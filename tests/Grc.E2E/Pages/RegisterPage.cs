using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using Grc.E2E.Helpers;

namespace Grc.E2E.Pages
{
    public class RegisterPage
    {
        private readonly IWebDriver _driver;
        private readonly WebDriverWait _wait;
        
        // Page elements
        private IWebElement EmailInput => _driver.FindElement(By.Id("Input_Email"));
        private IWebElement PasswordInput => _driver.FindElement(By.Id("Input_Password"));
        private IWebElement ConfirmPasswordInput => _driver.FindElement(By.Id("Input_ConfirmPassword"));
        private IWebElement FirstNameInput => _driver.FindElement(By.Id("Input_FirstName"));
        private IWebElement LastNameInput => _driver.FindElement(By.Id("Input_LastName"));
        private IWebElement CompanyNameInput => _driver.FindElement(By.Id("Input_CompanyName"));
        private IWebElement RegisterButton => _driver.FindElement(By.XPath("//button[contains(@class, 'btn-primary') and contains(text(), 'Register')]"));
        private IWebElement LoginLink => _driver.FindElement(By.LinkText("Log in"));
        
        // Validation
        private IWebElement ValidationSummary => _driver.FindElement(By.ClassName("validation-summary-errors"));
        private bool HasValidationErrors => _driver.FindElements(By.ClassName("validation-summary-errors")).Count > 0;
        
        public RegisterPage(IWebDriver driver)
        {
            _driver = driver;
            _wait = new WebDriverWait(driver, TimeSpan.FromSeconds(TestConfig.ImplicitWaitSeconds));
        }

        public void NavigateTo()
        {
            _driver.Navigate().GoToUrl($"{TestConfig.BaseUrl}/Account/Register");
            WaitForPageLoad();
        }

        public void FillRegistrationForm(string email, string password, string firstName, string lastName, string companyName = "")
        {
            EnterEmail(email);
            EnterPassword(password);
            EnterConfirmPassword(password);
            EnterFirstName(firstName);
            EnterLastName(lastName);
            
            if (!string.IsNullOrEmpty(companyName))
                EnterCompanyName(companyName);
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

        public void EnterConfirmPassword(string password)
        {
            _wait.Until(ExpectedConditions.ElementIsVisible(By.Id("Input_ConfirmPassword")));
            ConfirmPasswordInput.Clear();
            ConfirmPasswordInput.SendKeys(password);
        }

        public void EnterFirstName(string firstName)
        {
            _wait.Until(ExpectedConditions.ElementIsVisible(By.Id("Input_FirstName")));
            FirstNameInput.Clear();
            FirstNameInput.SendKeys(firstName);
        }

        public void EnterLastName(string lastName)
        {
            _wait.Until(ExpectedConditions.ElementIsVisible(By.Id("Input_LastName")));
            LastNameInput.Clear();
            LastNameInput.SendKeys(lastName);
        }

        public void EnterCompanyName(string companyName)
        {
            try
            {
                _wait.Until(ExpectedConditions.ElementIsVisible(By.Id("Input_CompanyName")));
                CompanyNameInput.Clear();
                CompanyNameInput.SendKeys(companyName);
            }
            catch
            {
                // Company name field might be optional
            }
        }

        public void ClickRegister()
        {
            RegisterButton.Click();
        }

        public void ClickLogin()
        {
            LoginLink.Click();
        }

        public bool IsRegistrationSuccessful()
        {
            try
            {
                // Wait for redirect after successful registration
                _wait.Until(driver => driver.Url.Contains("/Account/RegisterConfirmation") || 
                                      driver.Url.Contains("/Dashboard") || 
                                      !driver.Url.Contains("/Account/Register"));
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

        public bool IsOnRegisterPage()
        {
            return _driver.Url.Contains("/Account/Register");
        }

        private void WaitForPageLoad()
        {
            _wait.Until(driver => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));
        }
    }
}
