using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi;

namespace AzFunc4DevOps.AzureDevOps
{
    /// <summary>
    /// When returned from a Function, changes the state of given Release Environment (aka Release Stage)
    /// </summary>
    public class ReleaseEnvironmentStatus : ReleaseEnvironmentUpdateMetadata
    {
        /// <summary>
        /// ID of the Release this Environment (Stage) belongs to.
        /// </summary>
        public int ReleaseId { get; private set; }

        /// <summary>
        /// ID of this Release Environment (Stage)
        /// </summary>
        public int ReleaseEnvironmentId { get; private set; }

        public ReleaseEnvironmentStatus(int releaseId, int releaseEnvironmentId)
        {
            this.ReleaseId = releaseId;
            this.ReleaseEnvironmentId = releaseEnvironmentId;
        }
    }
}