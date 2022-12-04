using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi;
using Microsoft.VisualStudio.Services.WebApi;

namespace AzFunc4DevOps.AzureDevOps
{
    /// <inheritdoc />
    public class TestPlanValueProvider : IValueBinder
    {
        /// <inheritdoc />
        public TestPlanValueProvider(VssConnectionFactory connFactory, TestPlanAttribute attr)
        {
            this._connection = connFactory.GetVssConnection(attr);
            this._project = attr.Project;
            this._id = int.Parse(attr.Id);
        }

        /// <inheritdoc />
        public Type Type => typeof(TestSuiteProxy);

        /// <inheritdoc />
        public async Task<object> GetValueAsync()
        {
            var client = await this._connection.GetClientAsync<TestPlanHttpClient>();

            var plan = await client.GetTestPlanByIdAsync(this._project, this._id);

            var proxy = TestPlanProxy.FromTestPlan(plan);

            return proxy;
        }

        /// <inheritdoc />
        public Task SetValueAsync(object value, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public string ToInvokeString()
        {
            return $"{this._project}-{this._id}";
        }       

        private readonly VssConnection _connection;
        private readonly string _project;
        private readonly int _id;
    }
}