using System;

namespace AzFunc4DevOps.AzureDevOps
{
    public static class Settings
    {
        public static string AZURE_DEVOPS_ORG_URL
        {
            get { return Environment.GetEnvironmentVariable(nameof(AZURE_DEVOPS_ORG_URL)); }
        }

        public static string AZURE_DEVOPS_PAT
        {
            get { return Environment.GetEnvironmentVariable(nameof(AZURE_DEVOPS_PAT)); }
        }
    }
}