Feature: Document API List
	API list feature

Scenario: Get a list of documents
Given I call the document service API
When I call the API to get a list of documents
Then a list of PDFs is returned with the following properties: name, location, file-size

Scenario: Reorder list
Given I have a list of PDFs
When I choose to re-order the list of PDFs
Then the list of PDFs is returned in the new order for subsequent calls to the API
 
Scenario: Reorder list with invalid list
Given I have a list of PDFs
When I choose to re-order the list of PDFs but the list is incomplete
Then the API returns a bad status code and message
