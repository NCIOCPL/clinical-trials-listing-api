using System.Threading.Tasks;

namespace NCI.OCPL.Api.CTSListingPages
{
    /// <summary>
    /// Interface definition for retrieving trial type data.
    /// </summary>
    public interface ITrialTypeQueryService
    {
        /// <summary>
        /// Retrieve a single TrialTypeInfo with a pretty-url name or identifier exactly matching the name parameter.
        /// </summary>
        /// <param name="name">The name - either the pretty-url name or identifier string - of the record to be retrieved.</param>
        /// <returns>A TrialTypeInfo object or null if an exact match is not found.</returns>
        Task<TrialTypeInfo> Get(string name);
    }
}