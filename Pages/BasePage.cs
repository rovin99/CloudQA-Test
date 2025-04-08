using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;
using System.IO;

namespace CloudQATestsMacLib.Pages
{
    /// <summary>
    /// Base class for all Page Objects providing common functionality
    /// </summary>
    public abstract class BasePage
    {
        protected readonly IWebDriver Driver;
        protected readonly WebDriverWait Wait;

        /// <summary>
        /// Base constructor for all page objects
        /// </summary>
        protected BasePage(IWebDriver driver)
        {
            Driver = driver;
            Wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));
        }

        /// <summary>
        /// Navigates to a specific URL
        /// </summary>
        public void NavigateTo(string url)
        {
            Console.WriteLine($"Navigating to: {url}");
            Driver.Navigate().GoToUrl(url);
        }

        /// <summary>
        /// Finds an element with a fallback strategy for enhanced robustness
        /// </summary>
        protected IWebElement FindElementWithFallback(By primaryLocator, By? fallbackLocator = null, string elementName = "Element")
        {
            Console.WriteLine($"Attempting to find {elementName} using primary locator: {primaryLocator}");
            
            try
            {
                // Try with primary locator first (typically ID)
                return Wait.Until(ExpectedConditions.ElementIsVisible(primaryLocator));
            }
            catch (WebDriverTimeoutException)
            {
                if (fallbackLocator != null)
                {
                    Console.WriteLine($"Primary locator failed. Trying fallback locator for {elementName}: {fallbackLocator}");
                    return Wait.Until(ExpectedConditions.ElementIsVisible(fallbackLocator));
                }
                Console.WriteLine($"Failed to find element {elementName}. No fallback provided.");
                throw;
            }
        }

        /// <summary>
        /// Safely enters text into an input field
        /// </summary>
        protected void EnterText(IWebElement element, string text, string elementName = "field")
        {
            Console.WriteLine($"Entering text '{text}' into {elementName}");
            element.Clear();
            element.SendKeys(text);
        }

        /// <summary>
        /// Safely clicks an element
        /// </summary>
        protected void ClickElement(IWebElement element, string elementName = "element")
        {
            Console.WriteLine($"Clicking {elementName}");
            Wait.Until(driver => {
                try {
                    element.Click();
                    return true;
                }
                catch (ElementClickInterceptedException) {
                    return false;
                }
            });
        }

        /// <summary>
        /// Takes a screenshot and saves it to the specified path
        /// </summary>
        public void TakeScreenshot(string fileName)
        {
            try
            {
                Console.WriteLine($"Taking screenshot: {fileName}");
                
                // Set a shorter timeout for screenshot to avoid hanging
                var screenshotDriver = Driver as ITakesScreenshot;
                if (screenshotDriver == null)
                {
                    Console.WriteLine("WARNING: Driver doesn't support screenshots");
                    return;
                }
                
                var screenshot = screenshotDriver.GetScreenshot();
                
                var screenshotDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Screenshots");
                
                if (!Directory.Exists(screenshotDirectory))
                {
                    Directory.CreateDirectory(screenshotDirectory);
                }
                
                var screenshotPath = Path.Combine(screenshotDirectory, $"{fileName}_{DateTime.Now:yyyyMMdd_HHmmss}.png");
                screenshot.SaveAsFile(screenshotPath);
                Console.WriteLine($"Screenshot saved to: {screenshotPath}");
            }
            catch (Exception ex)
            {
                // Log but don't fail the test for screenshot failures
                Console.WriteLine($"WARNING: Failed to take screenshot: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Checks if an element exists without waiting (non-blocking)
        /// </summary>
        protected bool ElementExists(By locator)
        {
            try
            {
                Driver.FindElement(locator);
                return true;
            }
            catch (NoSuchElementException)
            {
                return false;
            }
        }
    }
} 