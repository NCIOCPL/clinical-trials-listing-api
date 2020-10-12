using Nest;

namespace NCI.OCPL.Api.CTSListingPages
{
    /// <summary>
    /// Child class of ListingInfo containing the labels and overrides for the listing item.
    /// </summary>
    public class NameInfo
    {
        /// <summary>
        /// The name for the term.
        /// </summary>
        [Keyword(Name = "label")]
        public string Label {get;set;}

        /// <summary>
        /// The label, converted by the loader to a normalized form.
        /// </summary>
        [Keyword(Name = "normalized")]
        public string Normalized { get; set; }
    }
}
