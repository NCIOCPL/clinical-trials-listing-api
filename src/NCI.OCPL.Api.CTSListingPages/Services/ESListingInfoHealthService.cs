using System;
using System.Threading.Tasks;

using Elasticsearch.Net;
using Nest;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using NCI.OCPL.Api.Common;
using NCI.OCPL.Api.CTSListingPages.Models;

namespace NCI.OCPL.Api.CTSListingPages.Services
{
    /// <summary>
    /// Wrapper around ESHealthCheckService to allow checking of the Listing Info index.
    /// </summary>
    public class ESListingInfoHealthService : ESHealthCheckService, IListingInfoHealthService
    {
        /// <summary>
        /// Creates a new instance of a ESTrialTypeHealthService
        /// </summary>
        /// <param name="client">The client to be used for connections</param>
        /// <param name="apiOptionsAccessor">API configuration options</param>
        /// <param name="logger">Logger instance.</param>
        public ESListingInfoHealthService(IElasticClient client,
            IOptions<ListingPageAPIOptions> apiOptionsAccessor,
            ILogger<ESHealthCheckService> logger)
            : base(
                client,
                new ESAliasNameProvider() { Name = apiOptionsAccessor.Value.ListingInfoAliasName },
                logger
            )
        { }
    }
}
