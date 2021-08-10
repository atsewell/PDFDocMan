Feature: Document API Upload
	API upload feature

Scenario: Upload a good PDF
Given I have a PDF to upload
When I send the PDF, sample-pdf.pdf, to the API
Then it is uploaded successfully

Scenario: Upload a non-PDF named as such
Given I have a non-PDF to upload
When I send the non-PDF, Test-non-PDF.xlsx, to the API
Then the API does not accept the file and returns the appropriate messaging and status; Uploaded file does not have the PDF extension

Scenario: Upload a non-PDF named like one
Given I have a non-PDF to upload
When I send the non-PDF, Test-non-PDF2.pdf, to the API
Then the API does not accept the file and returns the appropriate messaging and status; Uploaded file does not appear to be a PDF
 
Scenario: Upload a PDF that is too large
Given I have a max PDF size of 5MB
When I send the PDF, sample-pdf-download-10-mb.pdf, to the API
Then the API does not accept the file and returns the appropriate messaging and status; Uploaded file size
