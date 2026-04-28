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
        public string[] ConceptId { get; set; }

        /// <summary>
        /// Data structure containing the name of the disease or intervention.
        /// </summary>
        public NameInfo Name { get; set; }

        /// <summary>
        /// Contains the document's browser-friendly path segment. NULL if none exists.
        /// </summary>
        public string PrettyUrlName { get; set; }
    }
}
