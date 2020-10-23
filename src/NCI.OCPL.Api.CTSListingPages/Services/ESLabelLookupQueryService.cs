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
    /// Elasticsearch implemenation of the service for retrieving label information.
    /// </summary>
    public class ESLabelLookupQueryService : ILabelLookupQueryService
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
        private readonly ILogger<ESLabelLookupQueryService> _logger;

        /// <summary>
        /// Constructor.
        /// </summary>
        public ESLabelLookupQueryService(IElasticClient client, IOptions<ListingPageAPIOptions> apiOptionsAccessor,
            ILogger<ESLabelLookupQueryService> logger)
        {
            _elasticClient = client;
            _apiOptions = apiOptionsAccessor.Value;
            _logger = logger;
        }

        /// <summary>
        /// Retrieve a single LabelInformation with a pretty-url name or identifier exactly matching the name parameter.
        /// </summary>
        /// <param name="name">The name - either the pretty-url name or identifier string - of the record to be retrieved.</param>
        /// <returns>A LabelInformation object or null if an exact match is not found.</returns>
        public async Task<LabelInformation> Get(string name)
        {
            // Set up the SearchRequest to send to elasticsearch.
            Indices index = Indices.Index(new string[] { this._apiOptions.LabelInformationAliasName });
            Types types = Types.Type(new string[] { "LabelInformation" });
            SearchRequest request = new SearchRequest(index, types)
            {
                Query = new TermQuery { Field = "pretty_url_name", Value = name.ToString() } ||
                        new TermQuery { Field = "id_string", Value = name.ToString() }
            };

            ISearchResponse<LabelInformation> response = null;
            try
            {
                response = await _elasticClient.SearchAsync<LabelInformation>(request);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error searching index: '{this._apiOptions.LabelInformationAliasName}'.");
                throw new APIInternalException("errors occured");
            }

            if (!response.IsValid)
            {
                String msg = $"Invalid response when searching for pretty URL name or identifier '{name}'.";
                _logger.LogError(msg);
                _logger.LogError(response.DebugInformation);
                throw new APIInternalException("errors occured");
            }

            LabelInformation labelInfo = null;

            // If there is are any records in the response, the lookup was successful.
            if (response.Total > 0)
            {
                labelInfo = response.Documents.First();

                if (response.Total > 1)
                {
                    _logger.LogWarning($"Found multiple records for pretty URL name or identifier '{name}'.");
                }
            }

            return labelInfo;
        }
    }
}