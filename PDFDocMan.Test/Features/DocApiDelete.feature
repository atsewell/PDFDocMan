Feature: Document API Delete
	API delete feature

Scenario: Delete a PDF
Given I have selected a PDF from the list API that I no longer require
When I request to delete the PDF, 1
Then the PDF is deleted and will no longer return from the list API and can no longer be downloaded from its location directly

Scenario: Delete a PDF that does not exist
Given I attempt to delete a file that does not exist
When I request to delete the non-existing pdf, 99
Then the API returns the Not found status and message
