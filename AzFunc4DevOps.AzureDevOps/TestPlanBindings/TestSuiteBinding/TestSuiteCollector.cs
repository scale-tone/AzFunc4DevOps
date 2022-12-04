using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi;
using Microsoft.VisualStudio.Services.WebApi;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

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
            this._orgUrl = string.IsNullOrWhiteSpace(attr.OrgUrl) ? Settings.AZFUNC4DEVOPS_AZURE_DEVOPS_ORG_URL : attr.OrgUrl;
            this._pat = string.IsNullOrWhiteSpace(attr.PersonalAccessToken) ? Settings.AZFUNC4DEVOPS_AZURE_DEVOPS_PAT : attr.PersonalAccessToken;
        }

        /// <inheritdoc />
        public async Task AddAsync(TestSuiteProxy suite, CancellationToken cancellationToken = default)
        {
            var client = await this._connection.GetClientAsync<TestPlanHttpClient>();

            // Something is broken in SDK, so by now will be using raw REST API calls.
            string basicCredentials = Convert.ToBase64String(System.Text.ASCIIEncoding.ASCII.GetBytes(string.Format("{0}:{1}", string.Empty, this._pat)));

            if (suite.OriginalJson == null)
            {
                // If ParentSuite is not set, making this Test Suite a direct child of the given Test Plan
                if (suite.ParentSuite == null)
                {
                    var plan = await client.GetTestPlanByIdAsync(this._project, suite.PlanId);

                    suite.ParentSuite = plan.RootSuite;
                }

                if (suite.SuiteType == TestSuiteType.None)
                {
                    suite.SuiteType = TestSuiteType.StaticTestSuite;
                }

                var newSuite = await client.CreateTestSuiteAsync(suite, this._project, suite.PlanId);

                suite.Id = newSuite.Id;
            }
            else if (!JToken.DeepEquals(suite.OriginalJson, JObject.FromObject(suite)))
            {
                string uri = $"{this._orgUrl.Trim('/')}/{this._project}/_apis/test/Plans/{suite.Plan.Id}/suites/{suite.Id}?api-version=5.0";

                using (var request = new HttpRequestMessage(new HttpMethod("PATCH"), uri))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Basic", basicCredentials);

                    string bodyJson = JsonConvert.SerializeObject(suite.ToUpdateParams(), new JsonSerializerSettings
                    {
                        ContractResolver = new CamelCasePropertyNamesContractResolver()
                    });

                    request.Content = new StringContent(bodyJson, Encoding.UTF8, "application/json");

                    using (var response = await HttpClient.SendAsync(request))
                    {
                        // TODO: add better error handling
                        response.EnsureSuccessStatusCode();
                    }
                }
            }

            if (JToken.DeepEquals(JArray.FromObject(suite.OriginalTestCases), JArray.FromObject(suite.TestCases)))
            {
                return;
            }

            if (suite.OriginalTestCases.Any())
            {
                // Dropping all previously existed test cases
                string ids = string.Join(",", suite.OriginalTestCases.Select(tc => tc.Id));

                string uri = $"{this._orgUrl.Trim('/')}/{this._project}/_apis/test/Plans/{suite.Plan.Id}/suites/{suite.Id}/testcases/{ids}?api-version=5.0";
                using (var request = new HttpRequestMessage(HttpMethod.Delete, uri))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Basic", basicCredentials);

                    using (var response = await HttpClient.SendAsync(request))
                    {
                        // TODO: add better error handling
                        response.EnsureSuccessStatusCode();
                    }
                }
            }

            // Adding test cases anew. Can't do it via a single call, because in that case the order will be lost (looks like the server always first _sorts_ ids to be added)
            // TODO: find a way to optimize (not just drop/add test cases, but do it in a smarter way)
            foreach (var testCase in suite.TestCases)
            {
                string uri = $"{this._orgUrl.Trim('/')}/{this._project}/_apis/test/Plans/{suite.PlanId}/suites/{suite.Id}/testcases/{testCase.Id}?api-version=5.0";
                using (var request = new HttpRequestMessage(HttpMethod.Post, uri))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Basic", basicCredentials);

                    using (var response = await HttpClient.SendAsync(request))
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

                        using (var response = await HttpClient.SendAsync(request))
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
        private readonly string _orgUrl;
        private readonly string _pat;
    }
}