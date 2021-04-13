namespace NCI.OCPL.Api.CTSListingPages.Tests
{
    public abstract class GetByPrettyUrlName_BaseScenario
    {
        /// <summary>
        /// The mock response from Elasticsearch.
        /// </summary>
        /// <value></value>
        public abstract string MockESResponse {get;}

        /// <summary>
        /// The ListingInfo record which the service is expected to return from the response.
        /// </summary>
        public abstract ListingInfo ExpectedData { get; }

        /// <summary>
        /// The number of times logging is expected to occur.
        /// </summary>
        public abstract int ExpectedNumberOfLoggingCalls { get; }
    }
}