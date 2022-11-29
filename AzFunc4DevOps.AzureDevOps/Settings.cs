using System;

namespace AzFunc4DevOps.AzureDevOps
{
    /// <summary>
    /// Config Settings
    /// </summary>
    public static class Settings
    {
        /// <summary>
        /// Azure DevOps Organization's URL.
        /// E.g. <example>"https://dev.azure.com/my-company"</example>
        /// </summary>
        public static string AZFUNC4DEVOPS_AZURE_DEVOPS_ORG_URL
        {
            get { return Environment.GetEnvironmentVariable(nameof(AZFUNC4DEVOPS_AZURE_DEVOPS_ORG_URL)); }
        }

        /// <summary>
        /// Azure DevOps PAT (Personal Access Token).
        /// </summary>
        public static string AZFUNC4DEVOPS_AZURE_DEVOPS_PAT
        {
            get { return Environment.GetEnvironmentVariable(nameof(AZFUNC4DEVOPS_AZURE_DEVOPS_PAT)); }
        }

        /// <summary>
        /// Polling interval in seconds. Min 0.1, Max 29. Defaults to 1.
        /// </summary>
        public static double AZFUNC4DEVOPS_POLL_INTERVAL_IN_SECONDS
        {
            get 
            {
                string str = Environment.GetEnvironmentVariable(nameof(AZFUNC4DEVOPS_POLL_INTERVAL_IN_SECONDS));

                if (!string.IsNullOrWhiteSpace(str))
                {
                    double val = double.Parse(str);

                    if (val > 0.1 && val < (HeartBeatTimerTrigger.TimerIntervalInSec / 2))
                    {
                        return val;
                    }
                }

                return 1;
            }
        }
    }
}