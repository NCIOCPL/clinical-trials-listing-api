using Nest;

namespace NCI.OCPL.Api.CTSListingPages
{
    /// <summary>
    /// Base class common to EvsRecord and OverrideRecord.
    ///
    /// This class implements the properties defined in IListingInfo, however it
    /// expressly does *NOT* "implement" IListingInfo. The intent is to enforce the
    /// use of classes implementing IListingInfo for serializiation, while providing
    /// a single class for maintaining the NEST attribute mapping data.
    /// </summary>
    abstract public class ListingInfo : IListingInfo
    {
        /// <summary>
        /// Notes the subclass which represents a particular Elasticsearch ListingInfo document.
        /// </summary>
        [Keyword(Name = "type")]
        public RecordType Type {get; set;}

        /// <summary>
        /// An array of one or more concept IDs which mapping to this disease or intervention.
        /// </summary>
        [Keyword(Name = "concept_id")]
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
