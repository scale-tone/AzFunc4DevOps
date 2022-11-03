using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi;
using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi.Clients;
using Newtonsoft.Json.Linq;

namespace AzFunc4DevOps.AzureDevOps
{
    /// <summary>
    /// Represents the <see cref="ReleaseEnvironment"/> (aka Release Stage) object
    /// </summary>
    public class ReleaseEnvironmentProxy : ReleaseEnvironment
    {
        /// <summary>
        /// Reference to parent Release
        /// </summary>
        public ReleaseProxy ReleaseProxy
        { 
            get 
            {
                if (this._releaseProxy == null)
                {
                    this._releaseProxy = this._client == null ? 
                        null : 
                        ReleaseProxy.FromRelease(this._client.GetReleaseAsync(this._project, this.ReleaseId).Result);
                }

                return this._releaseProxy;
            }
        }

        /// <summary>
        /// References to all other Environments (Stages) of this Release.
        /// Navigate to them by their name.
        /// </summary>
        public IDictionary<string, ReleaseEnvironmentProxy> AllEnvironments
        {
            get
            {
                if (this._environmentsMap == null)
                {
                    this._environmentsMap = this.ReleaseProxy?.Environments?.ToDictionary(
                        env => env.Name, 
                        env => ReleaseEnvironmentProxy.FromReleaseEnvironment(env, this._client, this._project)
                    );
                }

                return this._environmentsMap;
            }
        }

        /// <summary>
        /// Result Values of this Release Environment (Stage).
        /// To output a result value, create a shell (bash, cmd, etc.) task, that has "AzFunc4DevOps" in its name
        /// and prints "&lt;AzFunc4DevOps.MyResult&gt;my-value&lt;/AzFunc4DevOps.MyResult&gt;" to stdout.
        /// Then "MyResult"="my-value" will appear in this map.
        /// </summary>
        public IDictionary<string, string> ResultValues
        {
            get
            {
                if (this._resultValuesMap == null)
                {
                    this._resultValuesMap = this.TryExtractingResultValuesFromLogs();
                }

                return this._resultValuesMap;
            }
        }

        private static readonly string TaskMarker = Global.FunctionPrefix;

        // TODO: add backreference
        public static readonly Regex ResultValueRegex = new Regex($"<AzFunc4DevOps\\.([\\w-]+)>(.*)</AzFunc4DevOps\\.([\\w-]+)>", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private ReleaseHttpClient _client;
        private string _project;
        private ReleaseProxy _releaseProxy;
        private Dictionary<string, ReleaseEnvironmentProxy> _environmentsMap;
        private Dictionary<string, string> _resultValuesMap;

        internal static ReleaseEnvironmentProxy FromReleaseEnvironment(ReleaseEnvironment item, ReleaseHttpClient client, string project)
        {
            var jObject = JObject.FromObject(item);
            var proxy = jObject.ToObject<ReleaseEnvironmentProxy>();

            // Keeping this reference, in case client code wants to navigate to full release
            proxy._client = client;
            proxy._project = project;

            return proxy;
        }

        private Dictionary<string, string> TryExtractingResultValuesFromLogs()
        {
            var result = new Dictionary<string, string>();

            foreach (var phase in this.DeploySteps.SelectMany(s => s.ReleaseDeployPhases))
            {
                foreach (var task in phase.DeploymentJobs.SelectMany(j => j.Tasks).Where(t => t.Name.Contains(TaskMarker)))
                {
                    // Extracting everything that looks like '<AzFunc4DevOps.MyResult>MyResultValue</AzFunc4DevOps.MyResult>' from task logs
                    
                    var logStream = this._client.GetTaskLogAsync(this._project, this.ReleaseId, this.Id, phase.Id, task.Id).Result;
                    using(logStream)
                    using(var reader = new StreamReader(logStream))
                    {
                        string log = reader.ReadToEnd();

                        for (var match = ResultValueRegex.Match(log); match.Success; match = match.NextMatch())
                        {
                            var name = match.Groups[1].Value;
                            var value = match.Groups[2].Value;

                            result[name] = value;
                        }
                    }
                }
            }

            return result;
        }
    }
}