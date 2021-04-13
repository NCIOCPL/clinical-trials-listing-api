using System;
using System.Linq;

using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Nest;

using System.Threading.Tasks;
using NCI.OCPL.Api.Common;
using NCI.OCPL.Api.CTSListingPages.Models;

namespace NCI.OCPL.Api.CTSListingPages.Services
{
    /// <summary>
    /// Elasticsearch implemenation of the service for retrieving trial type data.
    /// </summary>
    public class ESTrialTypeQueryService : ITrialTypeQueryService
    {
        /// <summary>
        /// The elasticsearch client
        /// </summary>
        private IElasticClient _elasticClient;

        /// <summary>
        /// The API options.
        /// </summary>
        protected readonly ListingPageAPIOptions _apiOptions;

        /// <summary>
        /// A logger to use for logging
        /// </summary>
        private readonly ILogger<ESTrialTypeQueryService> _logger;

        /// <summary>
        /// Constructor.
        /// </summary>
        public ESTrialTypeQueryService(IElasticClient client, IOptions<ListingPageAPIOptions> apiOptionsAccessor,
            ILogger<ESTrialTypeQueryService> logger)
        {
            _elasticClient = client;
            _apiOptions = apiOptionsAccessor.Value;
            _logger = logger;
        }

        /// <summary>
        /// Retrieve a single TrialTypeInfo with a pretty-url name or identifier exactly matching the name parameter.
        /// </summary>
        /// <param name="name">The name - either the pretty-url name or identifier string - of the record to be retrieved.</param>
        /// <returns>A TrialTypeInfo object or null if an exact match is not found.</returns>
        public async Task<TrialTypeInfo> Get(string name)
        {
            // Set up the SearchRequest to send to elasticsearch.
            Indices index = Indices.Index(new string[] { this._apiOptions.TrialTypeInfoAliasName });
            SearchRequest request = new SearchRequest(index)
            {
                Query = new TermQuery { Field = "pretty_url_name", Value = name.ToString() } ||
                        new TermQuery { Field = "id_string", Value = name.ToString() }
            };

            ISearchResponse<TrialTypeInfo> response = null;
            try
            {
                response = await _elasticClient.SearchAsync<TrialTypeInfo>(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error searching index: '{this._apiOptions.TrialTypeInfoAliasName}'.");
                throw new APIInternalException("errors occured");
            }

            if (!response.IsValid)
            {
                String msg = $"Invalid response when searching for pretty URL name or identifier '{name}'.";
                _logger.LogError(msg);
                _logger.LogError(response.DebugInformation);
                throw new APIInternalException("errors occured");
            }

            TrialTypeInfo trialTypeInfo = null;

            // If there are any records in the response, the lookup was successful.
            if (response.Total > 0)
            {
                trialTypeInfo = response.Documents.First();

                if (response.Total > 1)
                {
                    _logger.LogWarning($"Found multiple records for pretty URL name or identifier '{name}'.");
                }
            }

            return trialTypeInfo;
        }
    }
}