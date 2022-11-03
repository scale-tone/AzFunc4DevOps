using System;

namespace AzFunc4DevOps.AzureDevOps
{
    public static class Settings
    {
        /// <summary>
        /// Azure DevOps Organization's URL.
        /// E.g. <example>"https://dev.azure.com/my-company"</example>
        /// </summary>
        public static string AZURE_DEVOPS_ORG_URL
        {
            get { return Environment.GetEnvironmentVariable(nameof(AZURE_DEVOPS_ORG_URL)); }
        }

        /// <summary>
        /// Azure DevOps PAT (Personal Access Token).
        /// </summary>
        public static string AZURE_DEVOPS_PAT
        {
            get { return Environment.GetEnvironmentVariable(nameof(AZURE_DEVOPS_PAT)); }
        }
    }
}