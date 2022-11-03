using System;
using Microsoft.Azure.WebJobs.Description;

namespace AzFunc4DevOps.AzureDevOps 
{
    /// <summary>
    /// Binds to an IEnumerable&lt;WorkItemProxy&gt;.
    /// Use <see cref="WiqlQueryWhereClause"/> property to specify a WIQL query for items to be returned.
    /// </summary>
    [Binding]    
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
    public class WorkItemsAttribute : GenericProjectBindingAttribute
    {
        /// <summary>
        /// WIQL WHERE filter for Work Items to be returned.
        /// E.g. <example>"[Work Item Type] = 'User Story'"</example>.
        /// <seealso href="https://learn.microsoft.com/en-us/azure/devops/boards/queries/wiql-syntax?view=azure-devops#specify-filter-clauses-where"/>
        /// </summary>
        [AutoResolve]
        public string WiqlQueryWhereClause { get; set; }
    }
}