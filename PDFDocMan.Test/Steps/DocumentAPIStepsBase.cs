using System;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using TechTalk.SpecFlow;
using PDFDocMan.Api;
using static PDFDocMan.Test.TestExtensions;

namespace PDFDocMan.Test.Steps
{
    public class DocumentAPIStepsBase
    {
        protected readonly string BaseDir = TestSetup.BaseDir;
        protected readonly ScenarioContext _scenarioContext;
        protected readonly TestWebApplicationFactory<Startup> _factory;
        protected readonly HttpClient _client;

        public DocumentAPIStepsBase(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
            _factory = new TestWebApplicationFactory<Startup>(Guid.NewGuid().ToString());
            _client = _factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        }

        protected string GetResponseMessage(HttpResponseMessage response)
        {
            var message = GetResponseContent(response);
            JsonDocument jdoc;
            try
            {
                jdoc = JsonDocument.Parse(message);
            }
            catch (Exception ex) when (ex.Message.Contains("invalid start"))
            {
                return message;
            }

            // Try to find the "errors" property in the response.
            var errors = jdoc.RootElement.EnumerateObject()
                  .Where(o => o.Value.ValueKind == JsonValueKind.Object && o.Name == "errors")
                  .FirstOrDefault();

            if (errors.Value.ValueKind == JsonValueKind.Undefined)
            {
                return $"Couldn't find 'errors' object in response JSON; full response is:\n{message}";
            }

            // Get all the field validation errors and their messages
            return String.Join('\n', errors.Value.EnumerateObject()
                    .Select(o => String.Concat(o.Name, ": ", String.Join(',', o.Value.EnumerateArray().Select(v => v.GetString())))));
        }

        protected string GetResponseContent(HttpResponseMessage response) => RunSync(() => response.Content.ReadAsStringAsync());
    }
}
