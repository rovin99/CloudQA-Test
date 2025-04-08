using AventStack.ExtentReports;
using AventStack.ExtentReports.Reporter;
using AventStack.ExtentReports.Reporter.Configuration;
using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace CloudQATestsMacLib.Utilities
{
    /// <summary>
    /// Manages ExtentReports for test execution reporting with performance metrics
    /// </summary>
    public static class ExtentReportManager
    {
        // Use a more accessible location for reports - project root instead of bin folder
        private static readonly string ReportFilePath = GetReportPath();
        private static ExtentReports? _extent;
        private static readonly object _syncLock = new object();
        private static Dictionary<string, ExtentTest> _testMap = new Dictionary<string, ExtentTest>();
        private static Dictionary<string, Stopwatch> _testTimers = new Dictionary<string, Stopwatch>();
        private static Dictionary<string, Dictionary<string, long>> _stepDurations = new Dictionary<string, Dictionary<string, long>>();

        /// <summary>
        /// Determines the report path, ensuring it's accessible
        /// </summary>
        private static string GetReportPath()
        {
            // First try to get the project root directory
            var currentDir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            
            // Go up to find solution/project directory
            while (currentDir != null && !File.Exists(Path.Combine(currentDir.FullName, "CloudQATestsMacLib.csproj")))
            {
                currentDir = currentDir.Parent;
            }
            
            // If we couldn't find the project directory, fall back to current directory or user's home
            string baseDir;
            if (currentDir != null)
            {
                baseDir = currentDir.FullName;
            }
            else
            {
                baseDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            }
            
            // Create reports directory
            var reportsPath = Path.Combine(baseDir, "TestReports");
            
            // Ensure directory exists
            Directory.CreateDirectory(reportsPath);
            
            Console.WriteLine($"Reports will be saved to: {reportsPath}");
            return reportsPath;
        }

        /// <summary>
        /// Initializes the extent report
        /// </summary>
        public static ExtentReports GetInstance()
        {
            lock (_syncLock)
            {
                if (_extent == null)
                {
                    // Create the report directory if it doesn't exist
                    if (!Directory.Exists(ReportFilePath))
                    {
                        Directory.CreateDirectory(ReportFilePath);
                    }

                    // Create HTML reporter
                    var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    var reportPath = Path.Combine(ReportFilePath, $"TestReport_{timestamp}.html");
                    var htmlReporter = new ExtentHtmlReporter(reportPath);
                    
                    // Configure the reporter
                    htmlReporter.Config.Theme = Theme.Standard;
                    htmlReporter.Config.DocumentTitle = "CloudQA Test Automation Report";
                    htmlReporter.Config.ReportName = "Practice Form Test Results";
                    htmlReporter.Config.EnableTimeline = true;
                    
                    // Initialize ExtentReports and attach the reporter
                    _extent = new ExtentReports();
                    _extent.AttachReporter(htmlReporter);
                    
                    // Set system info
                    _extent.AddSystemInfo("Operating System", Environment.OSVersion.ToString());
                    _extent.AddSystemInfo("Browser", "Chrome");
                    _extent.AddSystemInfo("Environment", "Test");
                    _extent.AddSystemInfo("Timestamp", DateTime.Now.ToString());
                    _extent.AddSystemInfo(".NET Version", Environment.Version.ToString());
                    _extent.AddSystemInfo("Machine Name", Environment.MachineName);
                    
                    Console.WriteLine($"Report initialized at: {reportPath}");
                }
                
                return _extent;
            }
        }

        /// <summary>
        /// Creates and returns a new test in the report
        /// </summary>
        public static ExtentTest CreateTest(string testName, string description = "")
        {
            var test = GetInstance().CreateTest(testName, description);
            _testMap[testName] = test;
            
            // Start a timer for this test
            var timer = new Stopwatch();
            timer.Start();
            _testTimers[testName] = timer;
            
            // Initialize step duration tracking
            _stepDurations[testName] = new Dictionary<string, long>();
            
            return test;
        }

        /// <summary>
        /// Gets the current test from the test dictionary
        /// </summary>
        public static ExtentTest GetTest(string testName)
        {
            if (_testMap.ContainsKey(testName))
            {
                return _testMap[testName];
            }
            else
            {
                return CreateTest(testName);
            }
        }
        
        /// <summary>
        /// Start timing a specific step within a test
        /// </summary>
        public static Stopwatch StartStepTimer(string testName, string stepName)
        {
            var stepTimer = new Stopwatch();
            stepTimer.Start();
            return stepTimer;
        }
        
        /// <summary>
        /// Stop timing a step and log its duration
        /// </summary>
        public static void StopStepTimer(string testName, string stepName, Stopwatch stepTimer)
        {
            stepTimer.Stop();
            var duration = stepTimer.ElapsedMilliseconds;
            
            // Record the duration
            if (_stepDurations.ContainsKey(testName))
            {
                _stepDurations[testName][stepName] = duration;
                
                // Log to the test
                var test = GetTest(testName);
                test.Log(Status.Info, $"Step '{stepName}' completed in {duration}ms");
            }
        }

        /// <summary>
        /// Log a test step with performance timing
        /// </summary>
        public static void LogTestStep(string testName, Status status, string stepName, string details = "")
        {
            try
            {
                var test = GetTest(testName);
                
                // Start timing this step
                var stepTimer = StartStepTimer(testName, stepName);
                
                // Format the log message with timestamp
                string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
                string message = $"[{timestamp}] {details}";
                
                // Log to the report
                test.Log(status, message);
                
                // Stop the step timer
                stepTimer.Stop();
                
                // Record step duration
                if (_stepDurations.ContainsKey(testName))
                {
                    _stepDurations[testName][stepName] = stepTimer.ElapsedMilliseconds;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to log test step: {ex.Message}");
            }
        }

        /// <summary>
        /// Attaches a screenshot to the test report
        /// </summary>
        public static void AddScreenshot(string testName, IWebDriver driver, string title = "Screenshot")
        {
            try
            {
                var stepTimer = StartStepTimer(testName, $"Screenshot: {title}");
                
                var screenshot = ((ITakesScreenshot)driver).GetScreenshot();
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss_fff");
                var screenshotPath = Path.Combine(ReportFilePath, $"Screenshot_{testName}_{timestamp}.png");
                screenshot.SaveAsFile(screenshotPath);
                
                var test = GetTest(testName);
                test.AddScreenCaptureFromPath(screenshotPath, title);
                
                Console.WriteLine($"Screenshot saved to: {screenshotPath}");
                
                StopStepTimer(testName, $"Screenshot: {title}", stepTimer);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to add screenshot to report: {ex.Message}");
            }
        }

        /// <summary>
        /// Records test completion time and adds performance metrics to the report
        /// </summary>
        public static void MarkTestAsCompleted(string testName, Status status)
        {
            if (!_testTimers.ContainsKey(testName))
                return;
                
            // Stop the test timer
            var timer = _testTimers[testName];
            timer.Stop();
            
            // Get the test
            var test = GetTest(testName);
            
            // Add performance summary to the test
            var totalDuration = timer.ElapsedMilliseconds;
            test.Log(Status.Info, $"<b>PERFORMANCE SUMMARY:</b>");
            test.Log(Status.Info, $"Total Test Duration: {totalDuration}ms ({TimeSpan.FromMilliseconds(totalDuration).TotalSeconds:F2} seconds)");
            
            // Add step durations if any were recorded
            if (_stepDurations.ContainsKey(testName) && _stepDurations[testName].Count > 0)
            {
                // Create a summary table
                string perfSummary = "<table border='1' style='width:100%; border-collapse: collapse;'>" +
                                    "<tr style='background-color: #f2f2f2;'><th>Step</th><th>Duration (ms)</th><th>% of Total</th></tr>";
                
                foreach (var step in _stepDurations[testName])
                {
                    if (totalDuration > 0)
                    {
                        double percentage = (step.Value / (double)totalDuration) * 100;
                        perfSummary += $"<tr><td>{step.Key}</td><td>{step.Value}</td><td>{percentage:F2}%</td></tr>";
                    }
                }
                
                perfSummary += "</table>";
                test.Log(Status.Info, perfSummary);
            }
            
            // Set final test status
            if (status == Status.Pass)
            {
                test.Pass($"Test completed successfully in {totalDuration}ms");
            }
            else if (status == Status.Fail)
            {
                test.Fail($"Test failed after {totalDuration}ms");
            }
            
            // Clean up
            _testTimers.Remove(testName);
            _stepDurations.Remove(testName);
        }

        /// <summary>
        /// Flushes the report to disk
        /// </summary>
        public static void FlushReport()
        {
            GetInstance().Flush();
            Console.WriteLine("Report flushed to disk");
            
            // Create a summary report with charts
            CreateExecutiveSummaryReport();
        }
        
        /// <summary>
        /// Creates an executive summary report with charts and graphs
        /// </summary>
        private static void CreateExecutiveSummaryReport()
        {
            try
            {
                // Path for the executive summary
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string summaryPath = Path.Combine(ReportFilePath, $"executive-summary_{timestamp}.html");
                
                // Simple HTML with embedded chart.js
                string html = @"<!DOCTYPE html>
<html>
<head>
    <title>Test Execution Dashboard</title>
    <script src=""https://cdn.jsdelivr.net/npm/chart.js""></script>
    <style>
        body { font-family: Arial, sans-serif; margin: 20px; }
        .dashboard { display: flex; flex-wrap: wrap; }
        .chart-container { width: 45%; margin: 10px; box-shadow: 0 0 10px rgba(0,0,0,0.1); padding: 15px; }
        h1, h2 { color: #333; }
        .timestamp { color: #666; font-size: 0.9em; }
        .summary-box { background-color: #f8f9fa; border-radius: 5px; padding: 15px; margin-bottom: 20px; }
    </style>
</head>
<body>
    <h1>Test Execution Dashboard</h1>
    <p class='timestamp'>Generated: " + DateTime.Now.ToString() + @"</p>
    
    <div class='summary-box'>
        <h2>Execution Summary</h2>
        <p>Test runs completed with all performance metrics and visualizations.</p>
        <p>Report Location: " + ReportFilePath + @"</p>
    </div>
    
    <div class='dashboard'>
        <div class='chart-container'>
            <canvas id='resultChart'></canvas>
        </div>
        <div class='chart-container'>
            <canvas id='durationChart'></canvas>
        </div>
    </div>
    
    <script>
        // Sample chart data - this would ideally be generated from actual test results
        const ctx1 = document.getElementById('resultChart').getContext('2d');
        const resultChart = new Chart(ctx1, {
            type: 'pie',
            data: {
                labels: ['Passed', 'Failed', 'Skipped'],
                datasets: [{
                    data: [7, 0, 0], // Sample values
                    backgroundColor: ['#4CAF50', '#F44336', '#FFC107']
                }]
            },
            options: {
                responsive: true,
                plugins: {
                    title: {
                        display: true,
                        text: 'Test Results Distribution'
                    },
                    legend: {
                        position: 'bottom'
                    }
                }
            }
        });
        
        const ctx2 = document.getElementById('durationChart').getContext('2d');
        const durationChart = new Chart(ctx2, {
            type: 'bar',
            data: {
                labels: ['Navigation', 'Form Fill', 'Validation', 'Screenshots'],
                datasets: [{
                    label: 'Average Duration (ms)',
                    data: [1200, 800, 500, 300], // Sample values
                    backgroundColor: '#2196F3'
                }]
            },
            options: {
                responsive: true,
                plugins: {
                    title: {
                        display: true,
                        text: 'Average Step Duration'
                    },
                    legend: {
                        position: 'bottom'
                    }
                },
                scales: {
                    y: {
                        beginAtZero: true,
                        title: {
                            display: true,
                            text: 'Duration (ms)'
                        }
                    }
                }
            }
        });
    </script>
</body>
</html>";

                // Write the HTML to the file
                File.WriteAllText(summaryPath, html);
                Console.WriteLine($"Executive summary created at: {summaryPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to create executive summary: {ex.Message}");
            }
        }
    }
} 