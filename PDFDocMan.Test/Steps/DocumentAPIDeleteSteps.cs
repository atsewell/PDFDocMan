using System.Net.Http;
using Microsoft.AspNetCore.Http;
using FluentAssertions;
using TechTalk.SpecFlow;
using static PDFDocMan.Test.TestExtensions;

namespace PDFDocMan.Test.Steps
{
    [Binding]
    public class DocumentAPIDeleteSteps : DocumentAPIStepsBase
    {
        public DocumentAPIDeleteSteps(ScenarioContext scenarioContext) : base(scenarioContext) { } 
    
        [Given(@"I have selected a PDF from the list API that I no longer require")]
        public void GivenIHaveSelectedAPDFFromTheListAPIThatINoLongerRequire()
        {
        }
        
        [Given(@"I attempt to delete a file that does not exist")]
        public void GivenIAttemptToDeleteAFileThatDoesNotExist()
        {
        }

        [When(@"I request to delete the PDF, (\d+)")]
        public void WhenIRequestToDeleteThePDF(int id)
        {
            _scenarioContext["id"] = id;
        }

        [When(@"I request to delete the non-existing pdf, (\d+)")]
        public void WhenIRequestToDeleteTheNon_ExistingPdf(int id)
        {
            _scenarioContext["id"] = id;
        }

        [Then(@"the PDF is deleted and will no longer return from the list API and can no longer be downloaded from its location directly")]
        public void ThenThePDFIsDeletedAndWillNoLongerReturnFromTheListAPIAndCanNoLongerBeDownloadedFromItsLocationDirectly()
        {
            DeleteFileAndCheckStatus();
        }
        
        [Then(@"the API returns the Not found status and message")]
        public void ThenTheAPIReturnsTheNotFoundStatusAndMessage()
        {
            DeleteFileAndCheckNotFoundStatus();
        }

        private void DeleteFileAndCheckStatus()
        {
            var response = DeleteFile();
            if ((int)response.StatusCode != StatusCodes.Status204NoContent)
            {
                throw new TestResponseException(GetResponseMessage(response));
            }
        }

        private void DeleteFileAndCheckNotFoundStatus()
        {
            var response = DeleteFile();
            if ((int)response.StatusCode == StatusCodes.Status404NotFound)
            {
                var msg = GetResponseMessage(response);
                msg.Should().Match("Document with id * not found");
            }
            else
            {
                throw new TestResponseException(GetResponseMessage(response));
            }
        }

        private HttpResponseMessage DeleteFile()
        {
            var id = (int)_scenarioContext["id"];
            return RunSync(() => _client.DeleteAsync($"docs/{id}"));
        }
    }
}
