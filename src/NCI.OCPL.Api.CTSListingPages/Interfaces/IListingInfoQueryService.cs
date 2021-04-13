using System.Threading.Tasks;

namespace NCI.OCPL.Api.CTSListingPages
{
    /// <summary>
    /// Interface definition for retrieving EVS concept naming information.
    /// </summary>
    public interface IListingInfoQueryService
    {
        /// <summary>
        /// Retrieve a single ListingInfo with a pretty-url name exactly matching the name parameter.
        /// </summary>
        /// <param name="prettyUrlName">The pretty-url name of the record to be retrieved.</param>
        /// <returns>A ListingInfo object or null if an exact match is not found.</returns>
        Task<ListingInfo> GetByPrettyUrlName(string prettyUrlName);

        /// <summary>
        /// Retrieve an array of ListingInfo records with a c-code (list) exactly
        /// or partially matching the ccode parameter.
        /// </summary>
        /// <param name="ccodes">The c-code list of the record(s) to be retrieved.</param>
        /// <returns>An array of ListingInfo objects or null if exact or partial matches are not found.</returns>
        Task<ListingInfo[]> GetByIds(string[] ccodes);
    }
}