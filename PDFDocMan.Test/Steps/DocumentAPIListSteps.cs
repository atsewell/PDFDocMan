using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Net.Http;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using FluentAssertions;
using TechTalk.SpecFlow;
using PDFDocMan.Api;
using static PDFDocMan.Test.TestExtensions;

namespace PDFDocMan.Test.Steps
{
    [Binding]
    public class DocumentAPIListSteps : DocumentAPIStepsBase
    {
        public DocumentAPIListSteps(ScenarioContext scenarioContext) : base(scenarioContext) { } 
    
        [Given(@"I call the document service API")]
        public void GivenICallTheDocumentServiceAPI()
        {
        }
        
        [Given(@"I have a list of PDFs")]
        public void GivenIHaveAListOfPDFs()
        {
        }
        
        [When(@"I call the API to get a list of documents")]
        public void WhenICallTheAPIToGetAListOfDocuments()
        {
        }
        
        [When(@"I choose to re-order the list of PDFs")]
        public void WhenIChooseToRe_OrderTheListOfPDFs()
        {
            _scenarioContext["ids"] = new int[] { 4, 1, 3, 2 };
        }
        
        [When(@"I choose to re-order the list of PDFs but the list is incomplete")]
        public void WhenIChooseToRe_OrderTheListOfPDFsButTheListIsIncomplete()
        {
            _scenarioContext["ids"] = new int[] { 4, 1 };
        }

        [Then(@"a list of PDFs is returned with the following properties: name, location, file-size")]
        public void ThenAListOfPDFsIsReturnedWithTheFollowingPropertiesNameLocationFile_Size()
        {
            GetListAndCheckStatus();
        }
        
        [Then(@"the list of PDFs is returned in the new order for subsequent calls to the API")]
        public void ThenTheListOfPDFsIsReturnedInTheNewOrderForSubsequentCallsToTheAPI()
        {
            PutReorderAndCheckStatus();
        }

        [Then(@"the API returns a bad status code and message")]
        public void ThenTheAPIReturnsABadStatusCodeAndMessage()
        {
            DownloadFileAndCheckNotFoundStatus();
        }

        private IEnumerable<ApiDoc> GetListAndCheckStatus()
        {
            var response = GetList();
            if ((int)response.StatusCode != StatusCodes.Status200OK)
            {
                throw new TestResponseException(GetResponseMessage(response));
            }

            var content = GetResponseContent(response);
            var adocs = ConvertToApiDocList(content);
            adocs.Count().Should().Be(4, "there are 4 documents in the database");
            // TODO: Check list content
            return adocs;
        }

        private void DownloadFileAndCheckNotFoundStatus()
        {
            var ids = (int[])_scenarioContext["ids"];
            var response = PutReorder(ids);
            if ((int)response.StatusCode == StatusCodes.Status400BadRequest)
            {
                var msg = GetResponseMessage(response);
                msg.Should().Match("Incorrect number of ids provided to reorder*");
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

        private HttpResponseMessage GetList()
        {
            return RunSync(() => _client.GetAsync($"docs"));
        }

        private IEnumerable<ApiDoc> ConvertToApiDocList(string content) => JsonSerializer.Deserialize<List<ApiDoc>>(content, new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        private void PutReorderAndCheckStatus()
        {
            var ids = (int[])_scenarioContext["ids"];
            var response = PutReorder(ids);
            if ((int)response.StatusCode != StatusCodes.Status200OK)
            {
                throw new TestResponseException(GetResponseMessage(response));
            }

            var content = GetResponseContent(response);
            var adocs = ConvertToApiDocList(content);
            adocs.Count().Should().Be(4, "there are 4 documents in the database");
            CheckOrder(adocs, ids);
            var newadocs = GetListAndCheckStatus();
            CheckOrder(adocs, ids);
        }

        private void CheckOrder(IEnumerable<ApiDoc> adocs, int[] ids)
        {
            var ada = adocs.ToArray();
            for(var i = 0; i < ids.Length; i++)
            {
                ada[i].Id.Should().Be(ids[i], $"the id {ada[i].Id} should be item {i + 1} in the list");
            }
        }

        private HttpResponseMessage PutReorder(int[] ids)
        {
            var content = JsonContent.Create(ids);
            return RunSync(() => _client.PutAsync($"docs/reorder", content));
        }
    }
}
