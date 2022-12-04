using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi;
using Newtonsoft.Json.Linq;
using TestSuite = Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi.TestSuite;

namespace AzFunc4DevOps.AzureDevOps
{
    /// <summary>
    /// Represents the 'Test Suite' object.
    /// </summary>
    public class TestSuiteProxy : TestSuite
    {
        private int _planId;

        /// <summary>
        /// Id of Test Plan this Test Suite belongs to
        /// </summary>
        public int PlanId 
        { 
            get 
            { 
                return this.Plan?.Id ?? this._planId; 
            }

            set
            {                
                this._planId = value;

                if (this.Plan != null)
                {
                    this.Plan.Id = value;
                }
            }
        }

        /// <summary>
        /// List of Test Cases in this Test Suite.
        /// </summary>
        public ICollection<TestCaseId> TestCases { get; private set; } = new List<TestCaseId>();

        /// <summary>
        /// Snapshot of this object, in JSON form. Used for change detection.
        /// </summary>
        public JObject OriginalJson { get; set; }

        internal List<TestCaseId> OriginalTestCases { get; private set; } = new List<TestCaseId>();

        /// <inheritdoc />
        public TestSuiteProxy()
        {
            this.DefaultConfigurations = new List<TestConfigurationReference>();
            this.DefaultTesters = new List<Microsoft.VisualStudio.Services.WebApi.IdentityRef>();
        }

        /// <inheritdoc />
        public static TestSuiteProxy FromTestSuite(TestSuite item, List<TestCaseId> testCases = null)
        {
            var jObject = JObject.FromObject(item);
            var proxy = jObject.ToObject<TestSuiteProxy>();

            if (proxy.DefaultConfigurations == null)
            {
                proxy.DefaultConfigurations = new List<TestConfigurationReference>();
            }

            if (proxy.DefaultTesters == null)
            {
                proxy.DefaultTesters = new List<Microsoft.VisualStudio.Services.WebApi.IdentityRef>();
            }

            // Preserving the original values, to be able to detect changes later
            proxy.OriginalJson = jObject;

            proxy.TestCases = testCases ?? new List<TestCaseId>();
            proxy.OriginalTestCases = proxy.TestCases.ToList();

            return proxy;
        }

        /// <inheritdoc />
        public static TestSuiteProxy FromTestSuiteReference(TestSuiteReference item, int planId)
        {
            var proxy = new TestSuiteProxy();

            proxy.Id = item.Id;
            proxy.Name = item.Name;
            proxy.PlanId = planId;

            if (proxy.DefaultConfigurations == null)
            {
                proxy.DefaultConfigurations = new List<TestConfigurationReference>();
            }

            if (proxy.DefaultTesters == null)
            {
                proxy.DefaultTesters = new List<Microsoft.VisualStudio.Services.WebApi.IdentityRef>();
            }

            proxy.OriginalJson = JObject.FromObject(proxy);

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
        /// <summary>
        /// Work Item ID of this Test Case.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// (Optional) IDs of Test Configurations assigned to this Test Case.
        /// </summary>
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