using System.Net.Http;
using Microsoft.AspNetCore.Http;
using FluentAssertions;
using TechTalk.SpecFlow;
using static PDFDocMan.Test.TestExtensions;

namespace PDFDocMan.Test.Steps
{
    [Binding]
    public class DocumentAPIDownloadSteps : DocumentAPIStepsBase
    {
        public DocumentAPIDownloadSteps(ScenarioContext scenarioContext) : base(scenarioContext) { } 
    
        [Given(@"I have chosen a PDF from the list API")]
        public void GivenIHaveChosenAPDFFromTheListAPI()
        {
        }
        
        [When(@"I request the location for one of the PDFs, (\d+)")]
        public void WhenIRequestTheLocationForOneOfThePDFs(int id)
        {
            _scenarioContext["id"] = id;
        }

        [When(@"I request the location for a PDF, (\d+), that does not exist")]
        public void WhenIRequestTheLocationForAPDFThatDoesNotExist(int id)
        {
            _scenarioContext["id"] = id;
        }

        [Then(@"The PDF is downloaded")]
        public void ThenThePDFIsDownloaded()
        {
            DownloadFileAndCheckStatus();
        }
        
        [Then(@"the API returns the Not Found status and message")]
        public void ThenTheAPIReturnsTheNotFoundStatusAndMessage()
        {
            DownloadFileAndCheckNotFoundStatus();
        }

        private void DownloadFileAndCheckStatus()
        {
            var response = DownloadFile();
            if ((int)response.StatusCode != StatusCodes.Status200OK)
            {
                throw new TestResponseException(GetResponseMessage(response));
            }

            // TODO: Check file content
        }

        private void DownloadFileAndCheckNotFoundStatus()
        {
            var response = DownloadFile();
            if ((int)response.StatusCode == StatusCodes.Status404NotFound)
            {
                var msg = GetResponseMessage(response);
                msg.Should().Match("Document with id * not found");
            }
            else
            {
                if ((int)response.StatusCode == StatusCodes.Status200OK)
                {
                    throw new TestResponseException("Unexpected success response");
                }
                else
                {
                    throw new TestResponseException(GetResponseMessage(response));
                }
            }
        }

        private HttpResponseMessage DownloadFile()
        {
            var id = (int)_scenarioContext["id"];
            return RunSync(() => _client.GetAsync($"docs/{id}"));
        }
    }
}
