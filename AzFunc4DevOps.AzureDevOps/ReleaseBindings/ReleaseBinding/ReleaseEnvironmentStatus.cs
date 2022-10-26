using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi;

namespace AzFunc4DevOps.AzureDevOps
{
    /// <summary>
    /// When returned from a Function, changes the state of given Release Environment (aka Release Stage)
    /// </summary>
    public class ReleaseEnvironmentStatus : ReleaseEnvironmentUpdateMetadata
    {
        public int ReleaseId { get; private set; }
        public int ReleaseEnvironmentId { get; private set; }

        public ReleaseEnvironmentStatus(int releaseId, int releaseEnvironmentId)
        {
            this.ReleaseId = releaseId;
            this.ReleaseEnvironmentId = releaseEnvironmentId;
        }
    }
}