using Newtonsoft.Json.Linq;

namespace NCI.OCPL.Api.CTSListingPages.Tests
{
    public abstract class GetByIds_BaseScenario
    {
        /// <summary>
        /// The mock response from Elasticsearch.
        /// </summary>
        /// <value></value>
        public abstract string MockESResponse {get;}

        /// <summary>
        /// The ListingResults object which the service is expected to return from the response.
        /// </summary>
        public abstract ListingInfo[] ExpectedData { get; }

        /// <summary>
        /// The number of times logging is expected to occur.
        /// </summary>
        public abstract int ExpectedNumberOfLoggingCalls { get; }
    }
}