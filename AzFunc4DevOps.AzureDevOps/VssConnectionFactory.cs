using System;
using System.Collections.Concurrent;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;

namespace AzFunc4DevOps.AzureDevOps
{
    /// <summary>
    /// Produces VssConnection objects
    /// </summary>
    public class VssConnectionFactory
    {
        private readonly ConcurrentDictionary<string, VssConnection> _connections = new ConcurrentDictionary<string, VssConnection>();

        /// <summary>
        /// Produces VssConnection objects
        /// </summary>
        public VssConnection GetVssConnection(GenericBindingAttribute attribute) 
        {
            string orgUrl = string.IsNullOrWhiteSpace(attribute.OrgUrl) ? Settings.AZFUNC4DEVOPS_AZURE_DEVOPS_ORG_URL : attribute.OrgUrl;

            if (!this._connections.TryGetValue(orgUrl, out var conn))
            {
                string pat = string.IsNullOrWhiteSpace(attribute.PersonalAccessToken) ? Settings.AZFUNC4DEVOPS_AZURE_DEVOPS_PAT : attribute.PersonalAccessToken;

                conn = new VssConnection(new Uri(Settings.AZFUNC4DEVOPS_AZURE_DEVOPS_ORG_URL), new VssBasicCredential(string.Empty, Settings.AZFUNC4DEVOPS_AZURE_DEVOPS_PAT));

                this._connections[orgUrl] = conn;
            }

            return conn;
        }
    }
}