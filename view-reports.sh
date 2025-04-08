#!/bin/bash

# Script to help open the latest test reports

# Set the directory where the reports are stored
REPORTS_DIR="./TestReports"

# Check if directory exists
if [ ! -d "$REPORTS_DIR" ]; then
  echo "Error: Reports directory ($REPORTS_DIR) not found!"
  echo "Run the tests first to generate reports."
  exit 1
fi

# Find the latest test report and executive summary
LATEST_REPORT=$(find "$REPORTS_DIR" -name "TestReport_*.html" -type f -print0 | xargs -0 ls -t | head -1)
LATEST_SUMMARY=$(find "$REPORTS_DIR" -name "executive-summary_*.html" -type f -print0 | xargs -0 ls -t | head -1)

# Check if we found any reports
if [ -z "$LATEST_REPORT" ]; then
  echo "No test reports found in $REPORTS_DIR"
  exit 1
fi

# Determine the browser to use based on OS
if [[ "$OSTYPE" == "darwin"* ]]; then
  # macOS
  BROWSER="open"
elif [[ "$OSTYPE" == "linux-gnu"* ]]; then
  # Linux
  if command -v xdg-open &> /dev/null; then
    BROWSER="xdg-open"
  else
    BROWSER="firefox"
  fi
else
  # Windows or other
  BROWSER="start"
fi

# Open the reports
echo "Opening the latest test report: $LATEST_REPORT"
$BROWSER "$LATEST_REPORT"

if [ ! -z "$LATEST_SUMMARY" ]; then
  echo "Opening the latest executive summary: $LATEST_SUMMARY"
  $BROWSER "$LATEST_SUMMARY"
fi

echo "-------------------------------------------"
echo "All Reports:"
find "$REPORTS_DIR" -name "*.html" -type f | sort -r 