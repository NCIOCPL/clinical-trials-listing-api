using Nest;

namespace NCI.OCPL.Api.CTSListingPages
{
    /// <summary>
    /// Contains the Overridden form of an EvsRecord.
    /// </summary>
    public class OverrideRecord : ListingInfo
    {
        /// <summary>
        /// A unique identifier for the OverrideRecord object.
        /// </summary>
        [Keyword(Name = "unique_id")]
        public string UniqueId { get; set; }
    }
}
