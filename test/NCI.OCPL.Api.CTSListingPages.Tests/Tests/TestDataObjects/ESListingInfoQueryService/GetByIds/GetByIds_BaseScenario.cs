using Newtonsoft.Json.Linq;

namespace NCI.OCPL.Api.CTSListingPages.Tests
{
    public abstract class GetByIds_BaseScenario
    {
        /// <summary>
        /// The list of CCodes to pass into the query.
        /// </summary>
        public abstract string[] InputCCodes {get;}

        /// <summary>
        /// The mock response from Elasticsearch.
        /// </summary>
        public abstract string MockESResponse {get;}

        /// <summary>
        /// The ListingResults object which the service is expected to return from the response.
        /// </summary>
        public abstract ListingInfo[] ExpectedData { get; }
    }
}
