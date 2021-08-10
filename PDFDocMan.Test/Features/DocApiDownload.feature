Feature: Document API Download
	API download feature

Scenario: Download a PDF
Given I have chosen a PDF from the list API
When I request the location for one of the PDFs, 1
Then The PDF is downloaded

Scenario: Download a PDF that does not exist
Given I have chosen a PDF from the list API
When I request the location for a PDF, 99, that does not exist
Then the API returns the Not Found status and message
