using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using Grc.E2E.Helpers;

namespace Grc.E2E.Pages
{
    public class InviteUserPage
    {
        private readonly IWebDriver _driver;
        private readonly WebDriverWait _wait;
        private readonly SelectElement _roleSelect;
        
        // Page elements
        private IWebElement EmailInput => _driver.FindElement(By.Id("invitation-email"));
        private IWebElement FirstNameInput => _driver.FindElement(By.Id("invitation-firstname"));
        private IWebElement LastNameInput => _driver.FindElement(By.Id("invitation-lastname"));
        private IWebElement RoleDropdown => _driver.FindElement(By.Id("invitation-role"));
        private IWebElement DepartmentInput => _driver.FindElement(By.Id("invitation-department"));
        private IWebElement MessageTextArea => _driver.FindElement(By.Id("invitation-message"));
        private IWebElement SendInvitationButton => _driver.FindElement(By.XPath("//button[contains(text(), 'Send Invitation')]"));
        private IWebElement CancelButton => _driver.FindElement(By.XPath("//button[contains(text(), 'Cancel')]"));
        
        // Bulk invite elements
        private IWebElement BulkInviteTab => _driver.FindElement(By.LinkText("Bulk Invite"));
        private IWebElement CsvUploadInput => _driver.FindElement(By.Id("csv-upload"));
        private IWebElement DownloadTemplateLink => _driver.FindElement(By.LinkText("Download CSV Template"));
        private IWebElement UploadAndInviteButton => _driver.FindElement(By.XPath("//button[contains(text(), 'Upload and Invite')]"));
        
        // Success/Error messages
        private IWebElement SuccessMessage => _driver.FindElement(By.ClassName("alert-success"));
        private IWebElement ErrorMessage => _driver.FindElement(By.ClassName("alert-danger"));
        private bool HasSuccessMessage => _driver.FindElements(By.ClassName("alert-success")).Count > 0;
        private bool HasErrorMessage => _driver.FindElements(By.ClassName("alert-danger")).Count > 0;
        
        // Invitation history
        private IWebElement InvitationHistoryTable => _driver.FindElement(By.Id("invitation-history"));
        
        public InviteUserPage(IWebDriver driver)
        {
            _driver = driver;
            _wait = new WebDriverWait(driver, TimeSpan.FromSeconds(TestConfig.ImplicitWaitSeconds));
        }

        public void NavigateTo(string tenantSlug = null)
        {
            var url = string.IsNullOrEmpty(tenantSlug) 
                ? $"{TestConfig.BaseUrl}/Users/Invite"
                : $"{TestConfig.BaseUrl}/t/{tenantSlug}/Users/Invite";
                
            _driver.Navigate().GoToUrl(url);
            WaitForPageLoad();
        }

        public void SendInvitation(string email, string firstName, string lastName, string role, string message = "")
        {
            EnterEmail(email);
            EnterFirstName(firstName);
            EnterLastName(lastName);
            SelectRole(role);
            
            if (!string.IsNullOrEmpty(message))
                EnterMessage(message);
            
            ClickSendInvitation();
        }

        public void EnterEmail(string email)
        {
            _wait.Until(ExpectedConditions.ElementIsVisible(By.Id("invitation-email")));
            EmailInput.Clear();
            EmailInput.SendKeys(email);
        }

        public void EnterFirstName(string firstName)
        {
            _wait.Until(ExpectedConditions.ElementIsVisible(By.Id("invitation-firstname")));
            FirstNameInput.Clear();
            FirstNameInput.SendKeys(firstName);
        }

        public void EnterLastName(string lastName)
        {
            _wait.Until(ExpectedConditions.ElementIsVisible(By.Id("invitation-lastname")));
            LastNameInput.Clear();
            LastNameInput.SendKeys(lastName);
        }

        public void SelectRole(string role)
        {
            _wait.Until(ExpectedConditions.ElementIsVisible(By.Id("invitation-role")));
            var selectElement = new SelectElement(RoleDropdown);
            selectElement.SelectByText(role);
        }

        public void EnterDepartment(string department)
        {
            try
            {
                DepartmentInput.Clear();
                DepartmentInput.SendKeys(department);
            }
            catch
            {
                // Department field might be optional
            }
        }

        public void EnterMessage(string message)
        {
            _wait.Until(ExpectedConditions.ElementIsVisible(By.Id("invitation-message")));
            MessageTextArea.Clear();
            MessageTextArea.SendKeys(message);
        }

        public void ClickSendInvitation()
        {
            SendInvitationButton.Click();
        }

        public void ClickCancel()
        {
            CancelButton.Click();
        }

        public void SwitchToBulkInvite()
        {
            BulkInviteTab.Click();
            Thread.Sleep(500); // Wait for tab switch
        }

        public void UploadCsvFile(string filePath)
        {
            _wait.Until(ExpectedConditions.ElementIsVisible(By.Id("csv-upload")));
            CsvUploadInput.SendKeys(filePath);
        }

        public void ClickUploadAndInvite()
        {
            UploadAndInviteButton.Click();
        }

        public void DownloadCsvTemplate()
        {
            DownloadTemplateLink.Click();
        }

        public bool IsInvitationSent()
        {
            try
            {
                _wait.Until(ExpectedConditions.ElementIsVisible(By.ClassName("alert-success")));
                return HasSuccessMessage;
            }
            catch
            {
                return false;
            }
        }

        public string GetSuccessMessage()
        {
            return HasSuccessMessage ? SuccessMessage.Text : string.Empty;
        }

        public string GetErrorMessage()
        {
            return HasErrorMessage ? ErrorMessage.Text : string.Empty;
        }

        public List<InvitationRecord> GetInvitationHistory()
        {
            var records = new List<InvitationRecord>();
            
            try
            {
                _wait.Until(ExpectedConditions.ElementIsVisible(By.Id("invitation-history")));
                var rows = InvitationHistoryTable.FindElements(By.TagName("tr")).Skip(1); // Skip header row
                
                foreach (var row in rows)
                {
                    var cells = row.FindElements(By.TagName("td"));
                    if (cells.Count >= 4)
                    {
                        records.Add(new InvitationRecord
                        {
                            Email = cells[0].Text,
                            Name = cells[1].Text,
                            Role = cells[2].Text,
                            Status = cells[3].Text,
                            SentDate = cells.Count > 4 ? cells[4].Text : "",
                            Actions = cells.Count > 5 ? cells[5].Text : ""
                        });
                    }
                }
            }
            catch
            {
                // Invitation history might not be available
            }
            
            return records;
        }

        public bool ResendInvitation(string email)
        {
            try
            {
                var resendButton = _driver.FindElement(By.XPath($"//tr[contains(., '{email}')]//button[contains(text(), 'Resend')]"));
                resendButton.Click();
                Thread.Sleep(1000); // Wait for action
                return IsInvitationSent();
            }
            catch
            {
                return false;
            }
        }

        public bool CancelInvitation(string email)
        {
            try
            {
                var cancelButton = _driver.FindElement(By.XPath($"//tr[contains(., '{email}')]//button[contains(text(), 'Cancel')]"));
                cancelButton.Click();
                
                // Handle confirmation dialog if present
                try
                {
                    var confirmButton = _driver.FindElement(By.XPath("//button[contains(text(), 'Confirm')]"));
                    confirmButton.Click();
                }
                catch { }
                
                Thread.Sleep(1000); // Wait for action
                return true;
            }
            catch
            {
                return false;
            }
        }

        public List<string> GetAvailableRoles()
        {
            try
            {
                var selectElement = new SelectElement(RoleDropdown);
                return selectElement.Options.Select(o => o.Text).ToList();
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

    public class InvitationRecord
    {
        public string Email { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }
        public string Status { get; set; }
        public string SentDate { get; set; }
        public string Actions { get; set; }
    }
}
