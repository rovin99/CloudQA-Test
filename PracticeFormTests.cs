// --- Using Statements ---
// These lines import necessary code libraries (namespaces) we need.
using Microsoft.VisualStudio.TestTools.UnitTesting; // The MSTest framework for writing tests ([TestClass], [TestMethod], Assert)
using OpenQA.Selenium; // Core Selenium interfaces (IWebDriver, By, IWebElement)
using OpenQA.Selenium.Chrome; // Specific classes for using the Chrome browser
using OpenQA.Selenium.Support.UI; // Classes for Waits (WebDriverWait)
using System; // Basic .NET types (TimeSpan, Exception, Console)
using WebDriverManager; // The WebDriverManager library we installed
using WebDriverManager.DriverConfigs.Impl; // Specific config for Chrome within WebDriverManager
using SeleniumExtras.WaitHelpers; // The updated ExpectedConditions for waits
using System.Collections.Generic; // For Dictionary
using CloudQATestsMacLib.Pages; // Import our Pages namespace
using CloudQATestsMacLib.Utilities;
using CloudQATestsMacLib.Models;
using AventStack.ExtentReports;
using System.Diagnostics;

// --- Namespace ---
// Organizes our code. Should match the project name or be relevant.
namespace CloudQATestsMacLib
{
    // --- Test Class ---
    // The [TestClass] attribute tells the test runner that this class contains tests.
    [TestClass]
    public class PracticeFormTests
    {
        // --- Class Members (Variables) ---
        // These variables are accessible by all methods within this class.

        private IWebDriver? driver; // Marked as nullable to fix compiler warning
        private WebDriverWait? wait; // Marked as nullable to fix compiler warning
        private string testUrl = "https://app.cloudqa.io/home/AutomationPracticeForm"; // The URL of the page we are testing.
        private PracticeFormPage? practiceFormPage;
        private static ExtentTest? currentTest;
        private TestContext? testContextInstance;
        private Stopwatch? testStopwatch;

        /// <summary>
        /// Gets or sets the test context which provides information about current test run
        /// </summary>
        public TestContext TestContext
        {
            get { return testContextInstance!; }
            set { testContextInstance = value; }
        }

        // --- Element Locator Strategy Class ---
        // This class implements a fallback locator strategy for better robustness
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

        // --- Robust Locators with Fallback Strategy ---
        // Primary strategy is ID, fallback is other locator types (name, CSS, XPath)
        private readonly ElementLocator firstNameLocator = new(
            "First Name",
            By.Id("fname"),
            By.CssSelector("input[placeholder='First Name']")
        );
        
        private readonly ElementLocator genderMaleLocator = new(
            "Gender (Male)",
            By.Id("male"),
            By.XPath("//label[contains(text(),'Male')]/preceding-sibling::input[@type='radio'] | //input[@type='radio' and @value='male']")
        );
        
        private readonly ElementLocator mobileLocator = new(
            "Mobile Number",
            By.Id("mobile"),
            By.CssSelector("input[placeholder*='Mobile']")
        );

        // --- Helper Methods ---
        // Find element with fallback strategy for improved robustness
        private IWebElement FindElementWithFallback(ElementLocator locator)
        {
            if (wait == null)
                throw new InvalidOperationException("WebDriverWait is not initialized");

            Console.WriteLine($"Attempting to find {locator.ElementName} using primary locator: {locator.PrimaryLocator}");
            
            try
            {
                // Try with primary locator first (ID)
                return wait.Until(ExpectedConditions.ElementIsVisible(locator.PrimaryLocator));
            }
            catch (WebDriverTimeoutException)
            {
                if (locator.FallbackLocator != null)
                {
                    Console.WriteLine($"Primary locator failed. Trying fallback locator for {locator.ElementName}: {locator.FallbackLocator}");
                    return wait.Until(ExpectedConditions.ElementIsVisible(locator.FallbackLocator));
                }
                throw;
            }
        }

        // --- Setup Method ---
        // The [TestInitialize] attribute means this method runs BEFORE EACH test method ([TestMethod]) in this class.
        [TestInitialize]
        public void Setup()
        {
            // Start overall test timing
            testStopwatch = Stopwatch.StartNew();
            
            // Create a new test in the report
            currentTest = ExtentReportManager.CreateTest(
                TestContext.TestName, 
                TestContext.Properties.Contains("Description") 
                    ? TestContext.Properties["Description"].ToString() 
                    : "");
            
            ExtentReportManager.LogTestStep(TestContext.TestName, Status.Info, "Setup", "Setting up test...");

            try
            {
                // Setup WebDriverManager
                var setupDriverTimer = ExtentReportManager.StartStepTimer(TestContext.TestName, "WebDriverManager Setup");
                ExtentReportManager.LogTestStep(TestContext.TestName, Status.Info, "WebDriverManager", "Setting up ChromeDriver using WebDriverManager...");
                new DriverManager().SetUpDriver(new ChromeConfig());
                ExtentReportManager.StopStepTimer(TestContext.TestName, "WebDriverManager Setup", setupDriverTimer);
                ExtentReportManager.LogTestStep(TestContext.TestName, Status.Pass, "WebDriverManager", "ChromeDriver setup complete.");

                // Chrome options
                var options = new ChromeOptions();
                options.AddArgument("--start-maximized");
                options.AddArgument("--disable-extensions");
                options.AddArgument("--disable-popup-blocking");
                options.AddArgument("--ignore-certificate-errors");

                // Initialize WebDriver
                var driverInitTimer = ExtentReportManager.StartStepTimer(TestContext.TestName, "WebDriver Initialization");
                ExtentReportManager.LogTestStep(TestContext.TestName, Status.Info, "WebDriver", "Initializing ChromeDriver...");
                driver = new ChromeDriver(options);
                ExtentReportManager.StopStepTimer(TestContext.TestName, "WebDriver Initialization", driverInitTimer);
                ExtentReportManager.LogTestStep(TestContext.TestName, Status.Pass, "WebDriver", "ChromeDriver initialized.");

                // Initialize Page Objects
                var pageInitTimer = ExtentReportManager.StartStepTimer(TestContext.TestName, "Page Object Initialization");
                practiceFormPage = new PracticeFormPage(driver);
                ExtentReportManager.StopStepTimer(TestContext.TestName, "Page Object Initialization", pageInitTimer);
                ExtentReportManager.LogTestStep(TestContext.TestName, Status.Pass, "Page Objects", "Page Objects initialized.");

                ExtentReportManager.LogTestStep(TestContext.TestName, Status.Info, "Setup", "Setup finished.");
            }
            catch (Exception ex)
            {
                ExtentReportManager.LogTestStep(TestContext.TestName, Status.Fatal, "Setup Error", $"Setup failed: {ex.Message}");
                throw;
            }
        }

        // --- Test Method ---
        // The [TestMethod] attribute marks this as an actual test case to be run.
        [TestMethod]
        [Description("Tests First Name, Gender (Male), and Mobile Number fields using Page Object Model.")]
        [DataTestMethod]
        [DataRow("John", "1234567890", true, DisplayName = "Male with 10-digit mobile")]
        [DataRow("Jane", "9876543210", false, DisplayName = "Female with 10-digit mobile")]
        [DataRow("Chris", "5551234567", true, DisplayName = "Male with 10-digit mobile starting with 555")]
        public void TestBasicFieldsWithDataDriven(string firstName, string mobileNumber, bool isMale)
        {
            ExtentReportManager.LogTestStep(TestContext.TestName, Status.Info, "Test Start", 
                $"Starting test with data: FirstName={firstName}, Mobile={mobileNumber}, IsMale={isMale}");

            try
            {
                // Navigate to the form page
                var navigationTimer = ExtentReportManager.StartStepTimer(TestContext.TestName, "Navigation");
                practiceFormPage!.NavigateToPracticeForm();
                ExtentReportManager.StopStepTimer(TestContext.TestName, "Navigation", navigationTimer);
                
                // Take screenshot of initial page state
                ExtentReportManager.LogTestStep(TestContext.TestName, Status.Info, "Screenshot", "Capturing initial form state");
                practiceFormPage.TakeScreenshot("01_InitialFormState");
                ExtentReportManager.AddScreenshot(TestContext.TestName, driver!, "Initial Form State");
                ExtentReportManager.LogTestStep(TestContext.TestName, Status.Info, "Navigation Complete", "Navigated to practice form page.");

                // Fill the form fields based on test data
                var formFillTimer = ExtentReportManager.StartStepTimer(TestContext.TestName, "Form Fill");
                if (isMale)
                {
                    practiceFormPage
                        .EnterFirstName(firstName)
                        .SelectMaleGender()
                        .EnterMobileNumber(mobileNumber);
                    
                    ExtentReportManager.LogTestStep(TestContext.TestName, Status.Info, "Data Entry", $"Entered first name: {firstName}");
                    ExtentReportManager.LogTestStep(TestContext.TestName, Status.Info, "Data Entry", "Selected Male gender");
                }
                else
                {
                    practiceFormPage
                        .EnterFirstName(firstName)
                        .SelectFemaleGender()
                        .EnterMobileNumber(mobileNumber);
                    
                    ExtentReportManager.LogTestStep(TestContext.TestName, Status.Info, "Data Entry", $"Entered first name: {firstName}");
                    ExtentReportManager.LogTestStep(TestContext.TestName, Status.Info, "Data Entry", "Selected Female gender");
                }
                
                ExtentReportManager.LogTestStep(TestContext.TestName, Status.Info, "Data Entry", $"Entered mobile number: {mobileNumber}");
                ExtentReportManager.StopStepTimer(TestContext.TestName, "Form Fill", formFillTimer);
                
                // Take screenshot after filling form
                ExtentReportManager.LogTestStep(TestContext.TestName, Status.Info, "Screenshot", "Capturing filled form state");
                practiceFormPage.TakeScreenshot("02_FilledFormState");
                ExtentReportManager.AddScreenshot(TestContext.TestName, driver!, "Filled Form State");

                // Verify the field values using the page object's getter methods
                var validationTimer = ExtentReportManager.StartStepTimer(TestContext.TestName, "Validation");
                ExtentReportManager.LogTestStep(TestContext.TestName, Status.Info, "Validation", "Verifying form field values...");
                
                // First Name verification
                string actualFirstName = practiceFormPage.GetFirstNameValue();
                ExtentReportManager.LogTestStep(TestContext.TestName, Status.Info, "Validation", 
                    $"Verifying First Name field value. Expected: '{firstName}', Actual: '{actualFirstName}'");
                Assert.AreEqual(firstName, actualFirstName, 
                    $"FAILURE: First Name field value mismatch. Expected '{firstName}' but got '{actualFirstName}'.");
                ExtentReportManager.LogTestStep(TestContext.TestName, Status.Pass, "Validation", "First Name Assertion Passed.");
                
                // Gender verification
                if (isMale)
                {
                    bool isMaleSelected = practiceFormPage.IsMaleGenderSelected();
                    ExtentReportManager.LogTestStep(TestContext.TestName, Status.Info, "Validation", 
                        $"Verifying Gender (Male) is selected. Result: {isMaleSelected}");
                    Assert.IsTrue(isMaleSelected, "FAILURE: Gender (Male) radio button was not selected after clicking.");
                    ExtentReportManager.LogTestStep(TestContext.TestName, Status.Pass, "Validation", "Gender (Male) Assertion Passed.");
                }
                else
                {
                    bool isFemaleSelected = practiceFormPage.IsFemaleGenderSelected();
                    ExtentReportManager.LogTestStep(TestContext.TestName, Status.Info, "Validation", 
                        $"Verifying Gender (Female) is selected. Result: {isFemaleSelected}");
                    Assert.IsTrue(isFemaleSelected, "FAILURE: Gender (Female) radio button was not selected after clicking.");
                    ExtentReportManager.LogTestStep(TestContext.TestName, Status.Pass, "Validation", "Gender (Female) Assertion Passed.");
                }
                
                // Mobile Number verification
                string actualMobile = practiceFormPage.GetMobileNumberValue();
                ExtentReportManager.LogTestStep(TestContext.TestName, Status.Info, "Validation", 
                    $"Verifying Mobile Number field value. Expected: '{mobileNumber}', Actual: '{actualMobile}'");
                Assert.AreEqual(mobileNumber, actualMobile, 
                    $"FAILURE: Mobile Number field value mismatch. Expected '{mobileNumber}' but got '{actualMobile}'.");
                ExtentReportManager.LogTestStep(TestContext.TestName, Status.Pass, "Validation", "Mobile Number Assertion Passed.");
                
                ExtentReportManager.StopStepTimer(TestContext.TestName, "Validation", validationTimer);
                
                // Mark test as completed with performance metrics
                ExtentReportManager.MarkTestAsCompleted(TestContext.TestName, Status.Pass);
            }
            catch (Exception ex)
            {
                // Take screenshot on failure
                practiceFormPage?.TakeScreenshot("ERROR_TestFailure");
                ExtentReportManager.AddScreenshot(TestContext.TestName, driver!, "Error Screenshot");
                
                ExtentReportManager.LogTestStep(TestContext.TestName, Status.Fail, "Error", $"Test failed: {ex.Message}");
                ExtentReportManager.LogTestStep(TestContext.TestName, Status.Fail, "StackTrace", ex.StackTrace ?? "No stack trace available");
                
                // Mark test as failed with performance metrics
                ExtentReportManager.MarkTestAsCompleted(TestContext.TestName, Status.Fail);
                
                Assert.Fail($"Test failed due to an exception: {ex.Message}\nStackTrace: {ex.StackTrace}");
            }
            finally
            {
                // Record total test execution time
                if (testStopwatch != null)
                {
                    testStopwatch.Stop();
                    ExtentReportManager.LogTestStep(TestContext.TestName, Status.Info, "Performance", 
                        $"Total execution time: {testStopwatch.ElapsedMilliseconds}ms");
                }
            }
        }

        [TestMethod]
        [Description("Tests complete form fill with various test data using data-driven approach.")]
        [DataTestMethod]
        [DynamicData(nameof(GetTestData), DynamicDataSourceType.Method)]
        public void TestCompleteFormWithDynamicData(FormTestData testData)
        {
            ExtentReportManager.LogTestStep(TestContext.TestName, Status.Info, "Test Start", 
                $"Starting test with dynamic data: FirstName={testData.FirstName}, Email={testData.Email}");
            
            try
            {
                // Navigate to the form page
                var navigationTimer = ExtentReportManager.StartStepTimer(TestContext.TestName, "Navigation");
                practiceFormPage!.NavigateToPracticeForm();
                ExtentReportManager.StopStepTimer(TestContext.TestName, "Navigation", navigationTimer);
                
                // Take screenshot before filling form
                ExtentReportManager.AddScreenshot(TestContext.TestName, driver!, "Empty Form");
                
                // Fill the entire form with test data
                var formFillTimer = ExtentReportManager.StartStepTimer(TestContext.TestName, "Complete Form Fill");
                practiceFormPage.FillFormWithTestData(
                    firstName: testData.FirstName,
                    lastName: testData.LastName,
                    email: testData.Email,
                    isMale: testData.IsMale,
                    mobile: testData.Mobile,
                    dob: testData.DateOfBirth
                );
                ExtentReportManager.StopStepTimer(TestContext.TestName, "Complete Form Fill", formFillTimer);
                
                ExtentReportManager.LogTestStep(TestContext.TestName, Status.Info, "Form Fill", "Form filled with test data");
                
                // Take screenshot after filling form
                ExtentReportManager.AddScreenshot(TestContext.TestName, driver!, "Completed Form");
                
                // Verify key fields
                var validationTimer = ExtentReportManager.StartStepTimer(TestContext.TestName, "Validation");
                
                Assert.AreEqual(testData.FirstName, practiceFormPage.GetFirstNameValue(), "First name verification failed");
                ExtentReportManager.LogTestStep(TestContext.TestName, Status.Pass, "Validation", "First name verified");
                
                if (testData.IsMale)
                {
                    Assert.IsTrue(practiceFormPage.IsMaleGenderSelected(), "Gender (Male) verification failed");
                    ExtentReportManager.LogTestStep(TestContext.TestName, Status.Pass, "Validation", "Male gender verified");
                }
                else
                {
                    Assert.IsTrue(practiceFormPage.IsFemaleGenderSelected(), "Gender (Female) verification failed");
                    ExtentReportManager.LogTestStep(TestContext.TestName, Status.Pass, "Validation", "Female gender verified");
                }
                
                Assert.AreEqual(testData.Mobile, practiceFormPage.GetMobileNumberValue(), "Mobile number verification failed");
                ExtentReportManager.LogTestStep(TestContext.TestName, Status.Pass, "Validation", "Mobile number verified");
                
                ExtentReportManager.StopStepTimer(TestContext.TestName, "Validation", validationTimer);
                
                // Mark test as completed with performance metrics
                ExtentReportManager.MarkTestAsCompleted(TestContext.TestName, Status.Pass);
            }
            catch (Exception ex)
            {
                // Take screenshot on failure
                ExtentReportManager.AddScreenshot(TestContext.TestName, driver!, "Error State");
                
                ExtentReportManager.LogTestStep(TestContext.TestName, Status.Fail, "Error", $"Test failed: {ex.Message}");
                ExtentReportManager.LogTestStep(TestContext.TestName, Status.Fail, "StackTrace", ex.StackTrace ?? "No stack trace available");
                
                // Mark test as failed with performance metrics
                ExtentReportManager.MarkTestAsCompleted(TestContext.TestName, Status.Fail);
                
                Assert.Fail($"Test failed due to an exception: {ex.Message}\nStackTrace: {ex.StackTrace}");
            }
            finally
            {
                // Record total test execution time
                if (testStopwatch != null)
                {
                    testStopwatch.Stop();
                    ExtentReportManager.LogTestStep(TestContext.TestName, Status.Info, "Performance", 
                        $"Total execution time: {testStopwatch.ElapsedMilliseconds}ms");
                }
            }
        }

        /// <summary>
        /// Provides dynamic test data for data-driven tests
        /// </summary>
        public static IEnumerable<object[]> GetTestData()
        {
            // Return different test data scenarios
            yield return new object[] 
            { 
                new FormTestData(
                    "John", "Doe", "john.doe@example.com", 
                    true, "1234567890", "01/15/1990") 
            };
            
            yield return new object[] 
            { 
                new FormTestData(
                    "Jane", "Smith", "jane.smith@example.com", 
                    false, "9876543210", "05/20/1985") 
            };
            
            yield return new object[] 
            { 
                new FormTestData(
                    "Robert", "Johnson", "robert@example.com", 
                    true, "5551234567", "12/31/1975") 
            };
            
            yield return new object[] 
            { 
                new FormTestData(
                    "Emma", "Watson", "emma@example.com", 
                    false, "7778889999", "04/15/1992") 
            };
        }

        // --- Cleanup Method ---
        // The [TestCleanup] attribute means this method runs AFTER EACH test method ([TestMethod]) in this class.
        // It runs even if the test method failed.
        [TestCleanup]
        public void Teardown()
        {
            ExtentReportManager.LogTestStep(TestContext.TestName, Status.Info, "Teardown", "Starting Teardown...");
            
            try
            {
                if (driver != null)
                {
                    var browserCloseTimer = ExtentReportManager.StartStepTimer(TestContext.TestName, "Browser Cleanup");
                    ExtentReportManager.LogTestStep(TestContext.TestName, Status.Info, "Teardown", "Closing browser...");
                    driver.Quit();
                    ExtentReportManager.StopStepTimer(TestContext.TestName, "Browser Cleanup", browserCloseTimer);
                    ExtentReportManager.LogTestStep(TestContext.TestName, Status.Pass, "Teardown", "Browser closed successfully.");
                }
                else
                {
                    ExtentReportManager.LogTestStep(TestContext.TestName, Status.Warning, "Teardown", "Driver was null, skipping Quit.");
                }
            }
            catch (Exception ex)
            {
                ExtentReportManager.LogTestStep(TestContext.TestName, Status.Warning, "Teardown Error", $"Error during teardown: {ex.Message}");
            }
            
            ExtentReportManager.LogTestStep(TestContext.TestName, Status.Info, "Teardown", "Teardown finished.");
        }

        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            // Initialize the ExtentReports instance before any tests run
            ExtentReportManager.GetInstance();
            Console.WriteLine("ExtentReports initialized with performance tracking.");
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            // Flush the report to disk after all tests have completed
            ExtentReportManager.FlushReport();
            Console.WriteLine("ExtentReports finalized and saved to disk with performance metrics.");
        }
    }
}