using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.VisualStudio.Services.WebApi;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AzFunc4DevOps.AzureDevOps
{
    /// <inheritdoc />
    public class TestSuiteCollector : IAsyncCollector<TestSuiteProxy>
    {
        /// <inheritdoc />
        public TestSuiteCollector(VssConnectionFactory connFactory, TestSuiteAttribute attr)
        {
            this._connection = connFactory.GetVssConnection(attr);
            this._project = attr.Project;
        }

        /// <inheritdoc />
        public async Task AddAsync(TestSuiteProxy suite, CancellationToken cancellationToken = default)
        {
            if (!JToken.DeepEquals(suite.OriginalJson, JObject.FromObject(suite)) )
            {
                // TODO: implement full update for suites
                throw new NotImplementedException("Updating test suites is not supported yet. You can only update TestSuiteProxy.TestCaseIds by now.");
            }

            if (JToken.DeepEquals(JArray.FromObject(suite.OriginalTestCases), JArray.FromObject(suite.TestCases)))
            {
                return;
            }

            // Something is broken in SDK, so by now will be using raw REST API calls.
            // TODO: find a way to optimize (not just drop/add test cases, but do it in a smarter way)

            string basicCredentials = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", string.Empty, Settings.AZFUNC4DEVOPS_AZURE_DEVOPS_PAT)));

            if (suite.OriginalTestCases.Any())
            {
                // Dropping all previously existed test cases
                string ids = string.Join(",", suite.OriginalTestCases.Select(tc => tc.Id));

                string uri = $"{Settings.AZFUNC4DEVOPS_AZURE_DEVOPS_ORG_URL.Trim('/')}/{this._project}/_apis/test/Plans/{suite.Plan.Id}/suites/{suite.Id}/testcases/{ids}?api-version=5.0";
                using (var request = new HttpRequestMessage(HttpMethod.Delete, uri))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Basic", basicCredentials);

                    using(var response = await HttpClient.SendAsync(request))
                    {
                        // TODO: add better error handling
                        response.EnsureSuccessStatusCode();
                    }
                }
            }

            // Adding test cases anew. Can't do it via a single call, because in that case the order will be lost (looks like the server always first _sorts_ ids to be added)
            foreach (var testCase in suite.TestCases)
            {
                string uri = $"{Settings.AZFUNC4DEVOPS_AZURE_DEVOPS_ORG_URL.Trim('/')}/{this._project}/_apis/test/Plans/{suite.Plan.Id}/suites/{suite.Id}/testcases/{testCase.Id}?api-version=5.0";
                using (var request = new HttpRequestMessage(HttpMethod.Post, uri))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Basic", basicCredentials);

                    using(var response = await HttpClient.SendAsync(request))
                    {
                        // TODO: add better error handling
                        response.EnsureSuccessStatusCode();
                    }
                }

                if (testCase.ConfigurationIds != null && testCase.ConfigurationIds.Any())
                {
                    // Also updating assigned configurations
                    // TODO: find a way to do it via a single request

                    using (var request = new HttpRequestMessage(new HttpMethod("PATCH"), uri))
                    {
                        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", basicCredentials);

                        var body = new
                        {
                            configurations = testCase.ConfigurationIds.Select(id => new { id })
                        };

                        request.Content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");

                        using(var response = await HttpClient.SendAsync(request))
                        {
                            // TODO: add better error handling
                            response.EnsureSuccessStatusCode();
                        }
                    }
                }
            }
        }

        /// <inheritdoc />
        public Task FlushAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        private static HttpClient HttpClient = new HttpClient();
        private readonly VssConnection _connection;
        private readonly string _project;
    }
}