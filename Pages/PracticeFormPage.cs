using OpenQA.Selenium;
using System;
using System.Collections.Generic;

namespace CloudQATestsMacLib.Pages
{
    /// <summary>
    /// Page Object for the Practice Form page
    /// </summary>
    public class PracticeFormPage : BasePage
    {
        // URL for the page
        private readonly string _url = "https://app.cloudqa.io/home/AutomationPracticeForm";

        // Locator class for defining robust locator strategies
        private class ElementLocator
        {
            public By PrimaryLocator { get; }
            public By? FallbackLocator { get; }
            public string ElementName { get; }

            public ElementLocator(string elementName, By primaryLocator, By? fallbackLocator = null)
            {
                ElementName = elementName;
                PrimaryLocator = primaryLocator;
                FallbackLocator = fallbackLocator;
            }
        }

        // Form field locators with fallback strategies
        private readonly ElementLocator _firstNameLocator = new(
            "First Name",
            By.Id("fname"),
            By.CssSelector("input[placeholder='First Name']")
        );

        private readonly ElementLocator _lastNameLocator = new(
            "Last Name",
            By.Id("lname"),
            By.CssSelector("input[placeholder='Last Name']")
        );

        private readonly ElementLocator _emailLocator = new(
            "Email",
            By.Id("email"),
            By.CssSelector("input[type='email']")
        );

        private readonly ElementLocator _genderMaleLocator = new(
            "Gender (Male)",
            By.Id("male"),
            By.XPath("//label[contains(text(),'Male')]/preceding-sibling::input[@type='radio'] | //input[@type='radio' and @value='male']")
        );

        private readonly ElementLocator _genderFemaleLocator = new(
            "Gender (Female)",
            By.Id("female"),
            By.XPath("//label[contains(text(),'Female')]/preceding-sibling::input[@type='radio'] | //input[@type='radio' and @value='female']")
        );

        private readonly ElementLocator _mobileLocator = new(
            "Mobile Number",
            By.Id("mobile"),
            By.CssSelector("input[placeholder*='Mobile']")
        );

        private readonly ElementLocator _dobLocator = new(
            "Date of Birth",
            By.Id("dob"),
            By.CssSelector("input[placeholder*='Date of Birth']")
        );

        private readonly ElementLocator _submitButtonLocator = new(
            "Submit Button",
            By.Id("submit"),
            By.XPath("//button[contains(text(),'Submit')]")
        );

        /// <summary>
        /// Constructor for PracticeFormPage
        /// </summary>
        /// <param name="driver">The WebDriver instance</param>
        public PracticeFormPage(IWebDriver driver) : base(driver)
        {
        }

        /// <summary>
        /// Navigates to the Practice Form page
        /// </summary>
        public PracticeFormPage NavigateToPracticeForm()
        {
            NavigateTo(_url);
            return this;
        }

        /// <summary>
        /// Gets an element using the robust fallback strategy
        /// </summary>
        private IWebElement GetElement(ElementLocator locator)
        {
            return FindElementWithFallback(locator.PrimaryLocator, locator.FallbackLocator, locator.ElementName);
        }

        /// <summary>
        /// Enters first name in the form
        /// </summary>
        public PracticeFormPage EnterFirstName(string firstName)
        {
            var element = GetElement(_firstNameLocator);
            EnterText(element, firstName, "First Name field");
            return this;
        }

        /// <summary>
        /// Enters last name in the form
        /// </summary>
        public PracticeFormPage EnterLastName(string lastName)
        {
            var element = GetElement(_lastNameLocator);
            EnterText(element, lastName, "Last Name field");
            return this;
        }

        /// <summary>
        /// Enters email in the form
        /// </summary>
        public PracticeFormPage EnterEmail(string email)
        {
            var element = GetElement(_emailLocator);
            EnterText(element, email, "Email field");
            return this;
        }

        /// <summary>
        /// Selects Male gender option
        /// </summary>
        public PracticeFormPage SelectMaleGender()
        {
            var element = GetElement(_genderMaleLocator);
            if (!element.Selected)
            {
                ClickElement(element, "Male radio button");
            }
            return this;
        }

        /// <summary>
        /// Selects Female gender option
        /// </summary>
        public PracticeFormPage SelectFemaleGender()
        {
            var element = GetElement(_genderFemaleLocator);
            if (!element.Selected)
            {
                ClickElement(element, "Female radio button");
            }
            return this;
        }

        /// <summary>
        /// Enters mobile number in the form
        /// </summary>
        public PracticeFormPage EnterMobileNumber(string mobileNumber)
        {
            var element = GetElement(_mobileLocator);
            EnterText(element, mobileNumber, "Mobile Number field");
            return this;
        }

        /// <summary>
        /// Enters date of birth in the form
        /// </summary>
        public PracticeFormPage EnterDateOfBirth(string dob)
        {
            var element = GetElement(_dobLocator);
            EnterText(element, dob, "Date of Birth field");
            return this;
        }

        /// <summary>
        /// Submits the form
        /// </summary>
        public PracticeFormPage SubmitForm()
        {
            var element = GetElement(_submitButtonLocator);
            ClickElement(element, "Submit button");
            return this;
        }

        // Verification methods for test assertions

        /// <summary>
        /// Gets the current value of the first name field
        /// </summary>
        public string GetFirstNameValue()
        {
            return GetElement(_firstNameLocator).GetAttribute("value") ?? string.Empty;
        }

        /// <summary>
        /// Gets the current value of the mobile number field
        /// </summary>
        public string GetMobileNumberValue()
        {
            return GetElement(_mobileLocator).GetAttribute("value") ?? string.Empty;
        }

        /// <summary>
        /// Checks if the male gender option is selected
        /// </summary>
        public bool IsMaleGenderSelected()
        {
            return GetElement(_genderMaleLocator).Selected;
        }

        /// <summary>
        /// Checks if the female gender option is selected
        /// </summary>
        public bool IsFemaleGenderSelected()
        {
            return GetElement(_genderFemaleLocator).Selected;
        }

        /// <summary>
        /// Fills the entire form with test data
        /// </summary>
        public PracticeFormPage FillFormWithTestData(
            string firstName = "Test",
            string lastName = "User",
            string email = "test@example.com",
            bool isMale = true,
            string mobile = "1234567890",
            string dob = "01/01/2000")
        {
            EnterFirstName(firstName);
            EnterLastName(lastName);
            EnterEmail(email);
            
            if (isMale)
                SelectMaleGender();
            else
                SelectFemaleGender();
                
            EnterMobileNumber(mobile);
            EnterDateOfBirth(dob);
            
            return this;
        }
    }
} 