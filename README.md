# Cloud QA Test Automation Framework

This project implements a robust Selenium-based test automation framework for testing web forms, specifically designed to maintain stability even when element properties change.

## Architecture

The project follows the **Page Object Model (POM)** design pattern to create a maintainable and reusable test framework:

```
CloudQATestsMacLib/
├── Pages/                    # Page Object classes
│   ├── BasePage.cs           # Base class with common functionality
│   └── PracticeFormPage.cs   # Practice form specific page object
├── Models/                   # Data models
│   └── FormTestData.cs       # Model for test data in data-driven tests
├── Utilities/                # Helper classes
│   └── ExtentReportManager.cs # Reporting utility with performance tracking
├── TestReports/              # Generated test reports and screenshots (not committed to source control)
├── PracticeFormTests.cs      # Test classes
└── README.md                 # This file
```

### Key Components

1. **BasePage** - Abstract base class that provides:
   - Robust element finding with fallback strategies
   - Safe interaction methods (clicks, text entry)
   - Screenshot capabilities
   - Navigation methods

2. **PracticeFormPage** - Specific page implementation that:
   - Encapsulates all form field interactions
   - Implements verification methods
   - Provides a fluent API for test readability

3. **FormTestData** - Model class that:
   - Structures test data for data-driven tests
   - Provides constructors for both minimal and complete test data sets

4. **ExtentReportManager** - Reporting utility that:
   - Generates HTML reports with test details
   - Captures screenshots at key points and test failures
   - Displays test steps, statuses, and execution times
   - Tracks performance metrics for test steps
   - Creates executive summary dashboards with charts
   - Records timestamps for all test activities
   - Saves reports in an easily accessible location

5. **PracticeFormTests** - Test classes that:
   - Use the Page Objects to interact with the UI
   - Implement test assertions
   - Focus on test logic rather than element interactions
   - Leverage data-driven testing patterns
   - Capture performance metrics at each step

## Key Features

### 1. Robust Element Location

The framework uses a dual-strategy approach for finding elements:
- Primary strategy: ID-based locators (fastest and most reliable)
- Fallback strategy: CSS selectors or XPath when IDs change

Example:
```csharp
private readonly ElementLocator _firstNameLocator = new(
    "First Name",
    By.Id("fname"),                              // Primary - ID
    By.CssSelector("input[placeholder='First Name']")  // Fallback - CSS
);
```

### 2. Screenshot Support

The framework captures screenshots:
- At key points during test execution
- On test failures for debugging
- With timestamps for organization
- Automatically embedded in test reports

### 3. Fluent API Design

Tests use a fluent interface pattern for readable test code:

```csharp
practiceFormPage
    .EnterFirstName("John")
    .SelectMaleGender()
    .EnterMobileNumber("1234567890");
```

### 4. Data-Driven Testing

The framework supports multiple testing approaches:
- Parameterized tests with `[DataTestMethod]` and `[DataRow]` attributes
- Dynamic data with custom data providers
- Strongly-typed test data using model classes

Example:
```csharp
[DataTestMethod]
[DataRow("John", "1234567890", true, DisplayName = "Male with 10-digit mobile")]
[DataRow("Jane", "9876543210", false, DisplayName = "Female with 10-digit mobile")]
public void TestBasicFieldsWithDataDriven(string firstName, string mobileNumber, bool isMale)
{
    // Test implementation
}
```

### 5. Comprehensive Performance Metrics

The framework automatically tracks timing information:
- Total test execution time
- Duration of individual test steps
- Percentage of time spent in each operation
- Timestamps for each action
- Performance summary tables in reports

Example performance data captured:
```
PERFORMANCE SUMMARY:
Total Test Duration: 15243ms (15.24 seconds)

| Step               | Duration (ms) | % of Total |
|-------------------|--------------|------------|
| Navigation         | 3254         | 21.35%     |
| Form Fill          | 5678         | 37.25%     |
| Validation         | 2341         | 15.36%     |
| Screenshot: Initial| 987          | 6.47%      |
| Screenshot: Final  | 1023         | 6.71%      |
| Browser Cleanup    | 1760         | 11.55%     |
```

### 6. Rich Reporting

Test execution is documented with detailed reports:
- HTML reports with ExtentReports
- Test execution details and durations
- Embedded screenshots for visual verification
- System information and environment details
- Color-coded pass/fail status
- Executive dashboard with charts and graphs
- Timestamp-based activity logging

### 7. Error Handling

Extensive error handling to ensure tests fail gracefully:
- Try/catch blocks in critical sections
- Detailed logging for troubleshooting
- Screenshot capture on failures
- Automatic report generation on failures

## Running the Tests

```bash
# Run all tests
dotnet test

# Run a specific test
dotnet test --filter "Name=TestBasicFieldsWithDataDriven"

# Run tests with a specific category
dotnet test --filter "Category=SmokeTest"
```

## Test Reports

Test reports are automatically generated in a `TestReports` folder in your project root directory:

```
/TestReports/
├── TestReport_20250408_123456.html      # Main test report with timestamp
├── executive-summary_20250408_123456.html  # Executive dashboard
└── Screenshot_TestName_20250408_123456.png # Test screenshots
```

When running tests, the console output will show the exact location where reports are saved:

```
Reports will be saved to: /path/to/your/project/TestReports
Report initialized at: /path/to/your/project/TestReports/TestReport_20250408_123456.html
```

The reports include:
- Test results with pass/fail status
- Execution time for each test
- Screenshots of the application
- Environment information
- Detailed logs of test steps
- Performance metrics for test operations
- Visual charts and graphs of test execution

### Viewing Reports

To view reports, simply open the HTML files in your browser after test execution:

1. Navigate to the `TestReports` folder in your project root
2. Open the most recent `TestReport_*.html` file for detailed test results
3. Open the most recent `executive-summary_*.html` file for the dashboard view

### Performance Dashboard

The executive summary provides:
- Test result distribution charts
- Performance metrics visualizations
- Overall execution summary
- Environment details
- Timing analysis by test step

## Maintenance Guidelines

### Adding New Form Fields

1. Add a new ElementLocator in the PracticeFormPage class
2. Create interaction methods for the new field
3. Add getter methods for verification
4. Update the test data model if needed

### Creating Tests for New Scenarios

1. Create a new test method in the PracticeFormTests class
2. For data-driven tests, add DataRow attributes or data provider methods
3. Use the page object methods to interact with the form
4. Add assertions to verify expected behavior

### Adding New Test Data

1. Add new entries to the GetTestData method
2. Or create new DataRow attributes for inline test data
3. Consider creating specialized data provider methods for different test categories

### Adding Performance Metrics for New Operations

1. Create a new step timer for the operation:
   ```csharp
   var newOperationTimer = ExtentReportManager.StartStepTimer(TestContext.TestName, "New Operation");
   ```

2. Stop the timer and record the duration:
   ```csharp
   ExtentReportManager.StopStepTimer(TestContext.TestName, "New Operation", newOperationTimer);
   ```

3. Performance metrics will automatically be included in the reports

## Benefits Over Traditional Approaches

1. **Resilience to UI Changes**: The dual-locator strategy means tests continue to work even when IDs change
2. **Improved Test Readability**: The fluent API creates readable, self-documenting tests
3. **Reduced Maintenance**: Changes to the UI only require updates in one place (the Page Object)
4. **Better Test Coverage**: Data-driven testing allows testing many scenarios with minimal code duplication
5. **Enhanced Debugging**: Rich reporting with screenshots makes it easier to diagnose test failures
6. **Performance Insights**: Timing metrics help identify slow operations and bottlenecks
7. **Better Test Organization**: Clear separation between test logic and page interaction details 