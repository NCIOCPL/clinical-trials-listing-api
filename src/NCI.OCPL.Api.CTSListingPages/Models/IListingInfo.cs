using Newtonsoft.Json;

namespace NCI.OCPL.Api.CTSListingPages
{
    /// <summary>
    /// Interface definining the fields common to all ListingPage entities.
    /// </summary>
    [JsonConverter(typeof(ListingInfoConverter))]
    public interface IListingInfo
    {
        /// <summary>
        /// Notes the subclass which represents a particular Elasticsearch ListingInfo document.
        /// </summary>
        RecordType Type { get; set; }

        /// <summary>
        /// An array of one or more concept IDs which mapping to this disease or intervention.
        /// </summary>
        string[] ConceptId { get; set; }

        /// <summary>
        /// Data structure containing the name of the disease or intervention.
        /// </summary>
        NameInfo Name { get; set; }

        /// <summary>
        /// Contains the document's browser-friendly path segment. NULL if none exists.
        /// </summary>
        string PrettyUrlName { get; set; }
    }
}
