using Microsoft.Azure.WebJobs.Description;

namespace AzFunc4DevOps.AzureDevOps 
{
    /// <inheritdoc />
    public abstract class GenericProjectBindingAttribute : GenericBindingAttribute
    {
        /// <summary>
        /// Team Project's name. 
        /// Required.
        /// </summary>
        [AutoResolve]
        public string Project { get; set; }
    }
}