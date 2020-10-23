namespace NCI.OCPL.Api.CTSListingPages.Tests
{
    public abstract class Get_BaseScenario
    {
        /// <summary>
        /// The mock response from Elasticsearch.
        /// </summary>
        /// <value></value>
        public abstract string MockESResponse { get; }

        /// <summary>
        /// The LabelInformation which the service is expected to return from the response.
        /// </summary>
        public abstract LabelInformation ExpectedData { get; }

        /// <summary>
        /// The error message which the service is expected to log.
        /// </summary>
        public abstract string ErrorMessage { get; }

        /// <summary>
        /// The number of times the service is expected to log an error message.
        /// </summary>
        public abstract int ExpectedLoggerCalls { get; }
    }
}