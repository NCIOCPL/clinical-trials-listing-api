using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;

using NCI.OCPL.Api.Common;
using NCI.OCPL.Api.CTSListingPages.Models;

namespace NCI.OCPL.Api.CTSListingPages.Services
{
    /// <summary>
    /// Elasticsearch implementation of the service for retrieving Listing Info documents.
    /// </summary>
    public class ESListingInfoQueryService : IListingInfoQueryService
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
        private readonly ILogger<ESListingInfoQueryService> _logger;

        /// <summary>
        /// Constructor.
        /// </summary>
        public ESListingInfoQueryService(ElasticsearchClient client, IOptions<ListingPageAPIOptions> apiOptionsAccessor,
            ILogger<ESListingInfoQueryService> logger)
        {
            _elasticClient = client;
            _apiOptions = apiOptionsAccessor.Value;
            _logger = logger;
        }

        /// <summary>
        /// Retrieve the name and URL data for an EVS concept with a pretty-url name exactly matching the name parameter.
        /// </summary>
        /// <param name="prettyUrlName">The pretty-url name of the record to be retrieved.</param>
        /// <returns>A ListingInfo object or null if an exact match is not found.</returns>
        public async Task<ListingInfo> GetByPrettyUrlName(string prettyUrlName)
        {
            SearchRequest request = new SearchRequest(this._apiOptions.ListingInfoAliasName)
            {
                Query = new TermQuery { Field = "pretty_url_name", Value = prettyUrlName },
                Size = 2 // We only need to know if there are 0, 1, or more than 1 records, so we can limit the response to 2 records.
            };

            SearchResponse<ListingInfo> response = null;
            try
            {
                response = await _elasticClient.SearchAsync<ListingInfo>(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error searching index: '{this._apiOptions.ListingInfoAliasName}'.");
                throw new APIInternalException("errors occurred");
            }

            if (!response.IsValidResponse)
            {
                String msg = $"Invalid response when searching for pretty URL name '{prettyUrlName}'.";
                _logger.LogError(msg);
                _logger.LogError(response.DebugInformation);
                throw new APIInternalException("errors occurred");
            }

            ListingInfo record = null;

            // If there are any records in the response, the lookup was successful.
            int total = response.Documents.Count;
            if (total > 0)
            {
                record = response.Documents.First();

                if (total > 1)
                {
                    _logger.LogWarning($"Found multiple records for pretty URL name '{prettyUrlName}'.");
                }
            }

            return record;
        }

        /// <summary>
        /// Retrieve the name and URL data for EVS concept(s) with a c-code (list) exactly or partially matching the name parameter.
        /// </summary>
        /// <param name="ccodes">The c-code list of the record to be retrieved.</param>
        /// <returns>An array of ListingInfo objects or null if exact or partial matches are not found.</returns>
        public async Task<ListingInfo[]> GetByIds(string[] ccodes)
        {
            Indices index = Indices.Index(_apiOptions.ListingInfoAliasName);
            SearchRequest request = new SearchRequest(index)
            {
                Query = new TermsSetQuery {
                    Field = "concept_id",
                    Terms = ccodes,
                    MinimumShouldMatchScript = new Script { Source = "params.num_terms"}
                }
            };

            SearchResponse<ListingInfo> response = null;
            try
            {
                response = await _elasticClient.SearchAsync<ListingInfo>(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error searching index: '{this._apiOptions.ListingInfoAliasName}'.");
                throw new APIInternalException("errors occurred");
            }

            if (!response.IsValidResponse)
            {
                String msg = $"Invalid response when searching for c-code(s) '{String.Join(",", ccodes)}'.";
                _logger.LogError(msg);
                _logger.LogError(response.DebugInformation);
                throw new APIInternalException("errors occurred");
            }

            ListingInfo[] results = null;

            if(response.Documents.Count > 0)
            {
                results = response.Documents.ToArray();
            }

            return results;
        }
    }
}
