using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi;
using Microsoft.VisualStudio.Services.WebApi;
using Newtonsoft.Json.Linq;

namespace AzFunc4DevOps.AzureDevOps
{
    /// <inheritdoc />
    public class TestPlanCollector : IAsyncCollector<TestPlanProxy>
    {
        /// <inheritdoc />
        public TestPlanCollector(VssConnectionFactory connFactory, TestPlanAttribute attr)
        {
            this._connection = connFactory.GetVssConnection(attr);
            this._project = attr.Project;
        }

        /// <inheritdoc />
        public async Task AddAsync(TestPlanProxy plan, CancellationToken cancellationToken = default)
        {
            var client = await this._connection.GetClientAsync<TestPlanHttpClient>();

            if (plan.OriginalJson == null)
            {
                var newPlan = await client.CreateTestPlanAsync(plan, this._project);
                
                plan.Id = newPlan.Id;
            }
            else if (!JToken.DeepEquals(plan.OriginalJson, JObject.FromObject(plan)))
            {
                await client.UpdateTestPlanAsync(plan, this._project, plan.Id);
            }
        }

        /// <inheritdoc />
        public Task FlushAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        private readonly VssConnection _connection;
        private readonly string _project;
    }
}