using System.Threading.Tasks;

namespace NCI.OCPL.Api.CTSListingPages
{
    /// <summary>
    /// Interface definition for retrieving EVS override information.
    /// </summary>
    public interface IOverridesQueryService
    {
        /// <summary>
        /// Retrieve a single OverrideRecord with a pretty-url name exactly matching the name parameter.
        /// </summary>
        /// <param name="prettyUrlName">The pretty-url name of the record to be retrieved.</param>
        /// <returns>An OverrideRecord object or null if an exact match is not found.</returns>
        Task<OverrideRecord> GetByPrettyUrlName(string prettyUrlName);
    }
}