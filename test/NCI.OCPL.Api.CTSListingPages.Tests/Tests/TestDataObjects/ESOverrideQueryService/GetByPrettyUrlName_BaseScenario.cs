using Newtonsoft.Json.Linq;

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
        /// The Override record which the service is expected to return from the response.
        /// </summary>
        public abstract OverrideRecord ExpectedData { get; }
    }
}