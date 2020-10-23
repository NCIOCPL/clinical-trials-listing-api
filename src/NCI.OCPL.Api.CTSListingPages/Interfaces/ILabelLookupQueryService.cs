using System.Threading.Tasks;

namespace NCI.OCPL.Api.CTSListingPages
{
    /// <summary>
    /// Interface definition for retrieving label information.
    /// </summary>
    public interface ILabelLookupQueryService
    {
        /// <summary>
        /// Retrieve a single LabelInformation with a pretty-url name or identifier exactly matching the name parameter.
        /// </summary>
        /// <param name="name">The name - either the pretty-url name or identifier string - of the record to be retrieved.</param>
        /// <returns>A LabelInformation object or null if an exact match is not found.</returns>
        Task<LabelInformation> Get(string name);
    }
}