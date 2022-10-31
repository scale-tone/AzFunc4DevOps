using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi;
using Newtonsoft.Json.Linq;
using TestSuite = Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi.TestSuite;

namespace AzFunc4DevOps.AzureDevOps
{
    /// <summary>
    /// Represents the 'Test Suite' entity.
    /// </summary>
    public class TestSuiteProxy : TestSuite
    {
        public JObject OriginalJson { get; protected set; }
        internal List<TestCaseId> OriginalTestCases { get; private set; }
        public ICollection<TestCaseId> TestCases { get; private set; }

        internal static TestSuiteProxy FromTestSuite(TestSuite item, List<TestCaseId> testCases = null)
        {
            var jObject = JObject.FromObject(item);
            var proxy = jObject.ToObject<TestSuiteProxy>();

            proxy.TestCases = testCases ?? new List<TestCaseId>();
            proxy.OriginalTestCases = proxy.TestCases.ToList();

            // Preserving the original values, to be able to detect changes later
            proxy.OriginalJson = jObject;

            return proxy;
        }

        internal TestSuiteUpdateParams ToUpdateParams()
        {
            return new TestSuiteUpdateParams
            {
                Name = this.Name,
                ParentSuite = this.ParentSuite,
                QueryString = this.QueryString,
                InheritDefaultConfigurations = this.InheritDefaultConfigurations,
                DefaultConfigurations = this.DefaultConfigurations,
                DefaultTesters = this.DefaultTesters,
                Revision = this.Revision
            };
        }
    }

    /// <summary>
    /// Combines Test Case ID and a list of Test Configuration Ids ('Windows', 'Linux' etc.) assigned to it.
    /// </summary>
    public class TestCaseId
    {
        public int Id { get; set; }
        public ICollection<int> ConfigurationIds { get; set; }

        public TestCaseId()
        {
            this.ConfigurationIds = new List<int>();
        }

        public TestCaseId(int id)
        {
            this.Id = id;
            this.ConfigurationIds = new List<int>();
        }
    }
}