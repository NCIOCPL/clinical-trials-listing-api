using Nest;

namespace NCI.OCPL.Api.CTSListingPages
{
    /// <summary>
    /// Contains the naming information for a single EVS concept.
    /// </summary>
    public class ListingInfo
    {
        /// <summary>
        /// An array of one or more concept IDs which mapping to this disease or intervention.
        /// </summary>
        [Keyword(Name = "concept_id")]
        public string[] ConceptId { get; set; }

        /// <summary>
        /// A unique identifier for the ListingInfo object.
        /// </summary>
        [Keyword(Name = "unique_id")]
        public string UniqueId { get; set; }

        /// <summary>
        /// Data structure containing the name of the disease or intervention.
        /// </summary>
        [Nested(Name = "name")]
        public NameInfo Name { get; set; }

        /// <summary>
        /// Contains the document's browser-friendly path segment. NULL if none exists.
        /// </summary>
        [Keyword(Name = "pretty_url_name")]
        public string PrettyUrlName { get; set; }

        /// <summary>
        /// Flag denoting whether this document should be used to satisfy a request when
        /// it only matches a portion of the requested C-Codes.
        /// </summary>
        [Boolean(Name = "exact_match")]
        public bool ExactMatch { get; set; }
    }
}
