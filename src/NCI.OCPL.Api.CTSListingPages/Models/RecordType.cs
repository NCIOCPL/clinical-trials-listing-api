using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace NCI.OCPL.Api.CTSListingPages
{
    /// <summary>
    /// Friendly names for denoting the type of a ListingInfo Elasticsearch document.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum RecordType
    {
        /// <summary>
        /// The document is an EvsRecord.
        /// </summary>
        EvsRecord,

        /// <summary>
        /// The document is an OverrideRecord.
        /// </summary>
        OverrideRecord
    }
}