using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi;
using Microsoft.VisualStudio.Services.WebApi;

namespace AzFunc4DevOps.AzureDevOps
{
    public class TestSuiteValueProvider : IValueBinder
    {
        public TestSuiteValueProvider(VssConnection connection, TestSuiteAttribute attr)
        {
            this._connection = connection;
            this._projectName = attr.ProjectName;
            this._planId = int.Parse(attr.PlanId);
            this._id = string.IsNullOrWhiteSpace(attr.Id) ? (int?)null : int.Parse(attr.Id);
        }

        public Type Type => typeof(TestSuiteProxy);

        public async Task<object> GetValueAsync()
        {
            var client = await this._connection.GetClientAsync<TestPlanHttpClient>();

            if (!this._id.HasValue)
            {
                // Fetching plan's root suite id
                // TODO: optimize so that this step is only executed once (now it happens at every function call)
                var plan = await client.GetTestPlanByIdAsync(this._projectName, this._planId);
                this._id = plan.RootSuite.Id;
            }

            var item = await client.GetTestSuiteByIdAsync(this._projectName, this._planId, this._id.Value);

            // TODO: check if there can be multiple pages returned by this method
            var cases = await client.GetTestCaseListAsync(this._projectName, this._planId, this._id.Value);

            var proxy = TestSuiteProxy.FromTestSuite(item, cases.Select(c => new TestCaseId {

                Id = c.workItem.Id,
                ConfigurationIds = c.PointAssignments.Select(p => p.ConfigurationId).ToList()

            }).ToList());

            return proxy;
        }

        public Task SetValueAsync(object value, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public string ToInvokeString()
        {
            return $"{this._projectName}-{this._planId}-{this._id}";
        }
        

        private readonly VssConnection _connection;
        private readonly string _projectName;
        private readonly int _planId;
        private int? _id;
    }
}