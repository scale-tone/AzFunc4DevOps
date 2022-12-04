using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi;
using Newtonsoft.Json.Linq;
using TestSuite = Microsoft.VisualStudio.Services.TestManagement.TestPlanning.WebApi.TestSuite;

namespace AzFunc4DevOps.AzureDevOps
{
    /// <summary>
    /// Represents the 'Test Plan' object.
    /// </summary>
    public class TestPlanProxy : TestPlan
    {
        /// <summary>
        /// Snapshot of this object, in JSON form. Used for change detection.
        /// </summary>
        public JObject OriginalJson { get; set; }

        /// <inheritdoc />
        public static TestPlanProxy FromTestPlan(TestPlan item)
        {
            var jObject = JObject.FromObject(item);
            var proxy = jObject.ToObject<TestPlanProxy>();

            // Preserving the original values, to be able to detect changes later
            proxy.OriginalJson = jObject;

            return proxy;
        }
    }
}