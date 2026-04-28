using System;
using System.Linq;

using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;

using System.Threading.Tasks;
using NCI.OCPL.Api.Common;
using NCI.OCPL.Api.CTSListingPages.Models;

namespace NCI.OCPL.Api.CTSListingPages.Services
{
    /// <summary>
    /// Elasticsearch implementation of the service for retrieving trial type data.
    /// </summary>
    public class ESTrialTypeQueryService : ITrialTypeQueryService
    {
        /// <summary>
        /// The elasticsearch client
        /// </summary>
        private ElasticsearchClient _elasticClient;

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
        public ESTrialTypeQueryService(ElasticsearchClient client, IOptions<ListingPageAPIOptions> apiOptionsAccessor,
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
            Indices index = Indices.Index(this._apiOptions.TrialTypeInfoAliasName );
            SearchRequest request = new SearchRequest(index)
            {
                Query = new BoolQuery
                {
                    Should = new Query[]
                    {
                        new TermQuery { Field = "pretty_url_name", Value = name },
                        new TermQuery { Field = "id_string", Value = name }
                    }
                },
                Size = 2 // We only need to know if there are 0, 1, or more than 1 matches.
            };

            SearchResponse<TrialTypeInfo> response = null;
            try
            {
                response = await _elasticClient.SearchAsync<TrialTypeInfo>(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error searching index: '{this._apiOptions.TrialTypeInfoAliasName}'.");
                throw new APIInternalException("errors occurred");
            }

            if (!response.IsValidResponse)
            {
                String msg = $"Invalid response when searching for pretty URL name or identifier '{name}'.";
                _logger.LogError(msg);
                _logger.LogError(response.DebugInformation);
                throw new APIInternalException("errors occurred");
            }

            TrialTypeInfo trialTypeInfo = null;

            // If there are any records in the response, the lookup was successful.
            int total = response.Documents.Count;
            if (total > 0)
            {
                trialTypeInfo = response.Documents.First();

                if (total > 1)
                {
                    _logger.LogWarning($"Found multiple records for pretty URL name or identifier '{name}'.");
                }
            }

            return trialTypeInfo;
        }
    }
}