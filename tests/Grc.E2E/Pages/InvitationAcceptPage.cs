using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using Grc.E2E.Helpers;

namespace Grc.E2E.Pages
{
    public class InvitationAcceptPage
    {
        private readonly IWebDriver _driver;
        private readonly WebDriverWait _wait;
        
        // Page elements
        private IWebElement TokenDisplay => _driver.FindElement(By.ClassName("invitation-token"));
        private IWebElement EmailDisplay => _driver.FindElement(By.ClassName("invitation-email"));
        private IWebElement InviterDisplay => _driver.FindElement(By.ClassName("invitation-from"));
        private IWebElement CompanyDisplay => _driver.FindElement(By.ClassName("invitation-company"));
        private IWebElement RoleDisplay => _driver.FindElement(By.ClassName("invitation-role"));
        
        // Form elements
        private IWebElement PasswordInput => _driver.FindElement(By.Id("Input_Password"));
        private IWebElement ConfirmPasswordInput => _driver.FindElement(By.Id("Input_ConfirmPassword"));
        private IWebElement FirstNameInput => _driver.FindElement(By.Id("Input_FirstName"));
        private IWebElement LastNameInput => _driver.FindElement(By.Id("Input_LastName"));
        private IWebElement PhoneInput => _driver.FindElement(By.Id("Input_Phone"));
        private IWebElement AcceptButton => _driver.FindElement(By.XPath("//button[contains(text(), 'Accept Invitation')]"));
        private IWebElement DeclineButton => _driver.FindElement(By.XPath("//button[contains(text(), 'Decline')]"));
        
        // Terms and conditions
        private IWebElement TermsCheckbox => _driver.FindElement(By.Id("Input_AcceptTerms"));
        private IWebElement TermsLink => _driver.FindElement(By.LinkText("Terms and Conditions"));
        
        // Validation
        private IWebElement ValidationSummary => _driver.FindElement(By.ClassName("validation-summary-errors"));
        private bool HasValidationErrors => _driver.FindElements(By.ClassName("validation-summary-errors")).Count > 0;
        
        // Status messages
        private IWebElement SuccessMessage => _driver.FindElement(By.ClassName("alert-success"));
        private IWebElement ErrorMessage => _driver.FindElement(By.ClassName("alert-danger"));
        private IWebElement ExpiredMessage => _driver.FindElement(By.ClassName("invitation-expired"));
        
        public InvitationAcceptPage(IWebDriver driver)
        {
            _driver = driver;
            _wait = new WebDriverWait(driver, TimeSpan.FromSeconds(TestConfig.ImplicitWaitSeconds));
        }

        public void NavigateTo(string invitationToken)
        {
            _driver.Navigate().GoToUrl($"{TestConfig.BaseUrl}/Invitation/Accept?token={invitationToken}");
            WaitForPageLoad();
        }

        public void NavigateToDirectLink(string fullUrl)
        {
            _driver.Navigate().GoToUrl(fullUrl);
            WaitForPageLoad();
        }

        public bool IsInvitationValid()
        {
            try
            {
                // Check if the invitation details are displayed
                _wait.Until(ExpectedConditions.ElementIsVisible(By.ClassName("invitation-email")));
                return true;
            }
            catch
            {
                // Check for expired message
                return !IsInvitationExpired();
            }
        }

        public bool IsInvitationExpired()
        {
            return _driver.FindElements(By.ClassName("invitation-expired")).Count > 0;
        }

        public string GetInvitationEmail()
        {
            try
            {
                return EmailDisplay.Text;
            }
            catch
            {
                return string.Empty;
            }
        }

        public string GetInviterName()
        {
            try
            {
                return InviterDisplay.Text;
            }
            catch
            {
                return string.Empty;
            }
        }

        public string GetCompanyName()
        {
            try
            {
                return CompanyDisplay.Text;
            }
            catch
            {
                return string.Empty;
            }
        }

        public string GetAssignedRole()
        {
            try
            {
                return RoleDisplay.Text;
            }
            catch
            {
                return string.Empty;
            }
        }

        public void AcceptInvitation(string password, string firstName, string lastName, string phone = "")
        {
            EnterPassword(password);
            EnterConfirmPassword(password);
            EnterFirstName(firstName);
            EnterLastName(lastName);
            
            if (!string.IsNullOrEmpty(phone))
                EnterPhone(phone);
            
            AcceptTerms();
            ClickAccept();
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
            try
            {
                _wait.Until(ExpectedConditions.ElementIsVisible(By.Id("Input_FirstName")));
                FirstNameInput.Clear();
                FirstNameInput.SendKeys(firstName);
            }
            catch
            {
                // First name might be pre-filled from invitation
            }
        }

        public void EnterLastName(string lastName)
        {
            try
            {
                _wait.Until(ExpectedConditions.ElementIsVisible(By.Id("Input_LastName")));
                LastNameInput.Clear();
                LastNameInput.SendKeys(lastName);
            }
            catch
            {
                // Last name might be pre-filled from invitation
            }
        }

        public void EnterPhone(string phone)
        {
            try
            {
                PhoneInput.Clear();
                PhoneInput.SendKeys(phone);
            }
            catch
            {
                // Phone field might be optional
            }
        }

        public void AcceptTerms()
        {
            try
            {
                if (!TermsCheckbox.Selected)
                    TermsCheckbox.Click();
            }
            catch
            {
                // Terms checkbox might not be present
            }
        }

        public void ClickAccept()
        {
            AcceptButton.Click();
        }

        public void ClickDecline()
        {
            DeclineButton.Click();
            
            // Handle confirmation dialog if present
            try
            {
                var confirmButton = _driver.FindElement(By.XPath("//button[contains(text(), 'Confirm')]"));
                confirmButton.Click();
            }
            catch { }
        }

        public void ViewTerms()
        {
            try
            {
                TermsLink.Click();
                
                // Switch to new window/tab if terms open in new window
                var windows = _driver.WindowHandles;
                if (windows.Count > 1)
                {
                    _driver.SwitchTo().Window(windows.Last());
                    Thread.Sleep(2000); // Read terms
                    _driver.Close();
                    _driver.SwitchTo().Window(windows.First());
                }
            }
            catch { }
        }

        public bool IsAcceptanceSuccessful()
        {
            try
            {
                // Wait for redirect after successful acceptance
                _wait.Until(driver => driver.Url.Contains("/Dashboard") || 
                                      driver.Url.Contains("/Welcome") || 
                                      driver.Url.Contains("/Account/Login"));
                return true;
            }
            catch
            {
                // Check for success message on same page
                return _driver.FindElements(By.ClassName("alert-success")).Count > 0;
            }
        }

        public string GetErrorMessage()
        {
            if (HasValidationErrors)
                return ValidationSummary.Text;
            
            try
            {
                return ErrorMessage.Text;
            }
            catch
            {
                return string.Empty;
            }
        }

        public string GetSuccessMessage()
        {
            try
            {
                return SuccessMessage.Text;
            }
            catch
            {
                return string.Empty;
            }
        }

        public bool IsPasswordStrengthIndicatorPresent()
        {
            return _driver.FindElements(By.ClassName("password-strength")).Count > 0;
        }

        public string GetPasswordStrength()
        {
            try
            {
                var strengthIndicator = _driver.FindElement(By.ClassName("password-strength"));
                return strengthIndicator.GetAttribute("data-strength") ?? strengthIndicator.Text;
            }
            catch
            {
                return string.Empty;
            }
        }

        private void WaitForPageLoad()
        {
            _wait.Until(driver => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));
        }
    }
}
