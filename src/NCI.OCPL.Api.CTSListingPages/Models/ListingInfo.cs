using Nest;

namespace NCI.OCPL.Api.CTSListingPages
{
    /// <summary>
    /// Abstract base class common to EvsRecord and OverrideRecord.
    /// </summary>
    abstract public class ListingInfo
    {
        /// <summary>
        /// An array of one or more concept IDs which mapping to this disease or intervention.
        /// </summary>
        [Keyword(Name = "label")]
        public string[] ConceptId {get;set;}

        /// <summary>
        /// Data structure containing the name of the disease or intervention.
        /// </summary>
        [Nested(Name = "name")]
        public NameInfo Name{get;set;}

        /// <summary>
        /// Contains the document's browser-friendly path segment. NULL if none exists.
        /// </summary>
        [Keyword(Name = "pretty_url_name")]
        public string PrettyUrlName { get; set; }
    }
}
