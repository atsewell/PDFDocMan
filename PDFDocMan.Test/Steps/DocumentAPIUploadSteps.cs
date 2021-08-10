using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Http;
using FluentAssertions;
using TechTalk.SpecFlow;
using static PDFDocMan.Test.TestExtensions;

namespace PDFDocMan.Test.Steps
{
    [Binding]
    public class DocumentAPIUploadSteps : DocumentAPIStepsBase
    {
        public DocumentAPIUploadSteps(ScenarioContext scenarioContext) : base(scenarioContext) { } 

        [Given(@"I have a PDF to upload")]
        public void GivenIHaveAPDFToUpload()
        {
        }
        
        [Given(@"I have a non-PDF to upload")]
        public void GivenIHaveANon_PdfToUpload()
        {
        }

        [Given(@"I have a max PDF size of 5MB")]
        public void GivenIHaveAMaxPdfSizeOfMB()
        {
        }

        [When(@"I send the PDF, (.*), to the API")]
        public void WhenISendThePdfToTheAPI(string filename)
        {
            _scenarioContext["filename"] = filename;
        }
        
        [When(@"I send the non-PDF, (.*), to the API")]
        public void WhenISendTheNon_PdfToTheAPI(string filename)
        {
            _scenarioContext["filename"] = filename;
        }

        [Then(@"it is uploaded successfully")]
        public void ThenItIsUploadedSuccessfully()
        {
            Action act = () => UploadFileAndCheckStatus();
            act.Should().NotThrow();
        }

        [Then(@"the API does not accept the file and returns the appropriate messaging and status; (.*)")]
        public void ThenTheAPIDoesNotAcceptTheFileAndReturnsTheAppropriateMessagingAndStatus(string msg)
        {
            Action act = () => UploadFileAndCheckStatus();
            act.Should().Throw<TestResponseException>().WithMessage($"*{msg}*");
        }

        private void UploadFileAndCheckStatus()
        {
            var filename = (string)_scenarioContext["filename"];
            var filePath = Path.Combine(BaseDir, filename);
            using var stream = File.OpenRead(filePath);
            using var form = new MultipartFormDataContent();
            using var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
            form.Add(fileContent, "file", filename);

            var response = RunSync(() => _client.PostAsync("docs", form));
            if ((int)response.StatusCode != StatusCodes.Status201Created)
            {
                throw new TestResponseException(GetResponseMessage(response));
            }
        }
    }
}
