using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using Elasticsearch.Net;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging.Testing;
using Moq;
using Nest;
using Newtonsoft.Json.Linq;
using Xunit;

using NCI.OCPL.Api.Common.Testing;
using NCI.OCPL.Api.CTSListingPages.Models;
using NCI.OCPL.Api.CTSListingPages.Services;

namespace NCI.OCPL.Api.CTSListingPages.Tests
{
    public partial class ESOverrideQueryServiceTest
    {
        /// <summary>
        /// Test to verify that Elasticsearch requests are being assembled correctly.
        /// </summary>
        [Fact]
        public async void GetByPrettyUrlName_TestRequestSetup()
        {
            const string theName = "recurrent-adult-brain";
            JObject expectedRequest = JObject.Parse(
@"{
    ""query"": {
        ""bool"": {
            ""must"": [
                { ""term"": { ""pretty_url_name"": { ""value"": ""recurrent-adult-brain"" } } },
                { ""term"": { ""type"":            { ""value"": ""OverrideRecord"" } } }
            ]
        }
    }
}
");

            Uri esURI = null;
            string esContentType = String.Empty;
            HttpMethod esMethod = HttpMethod.DELETE; // Basically, something other than the expected value.

            JObject requestBody = null;

            ElasticsearchInterceptingConnection conn = new ElasticsearchInterceptingConnection();
            conn.RegisterRequestHandlerForType<Nest.SearchResponse<OverrideRecord>>((req, res) =>
            {
                // We don't really care about the response for this test.
                res.Stream = MockEmptyResponse;
                res.StatusCode = 200;

                esURI = req.Uri;
                esContentType = req.ContentType;
                esMethod = req.Method;
                requestBody = conn.GetRequestPost(req);
            });

            // The URI does not matter, an InMemoryConnection never requests from the server.
            var pool = new SingleNodeConnectionPool(new Uri("http://localhost:9200"));

            var connectionSettings = new ConnectionSettings(pool, conn);
            IElasticClient client = new ElasticClient(connectionSettings);

            // Setup the mocked Options
            IOptions<ListingPageAPIOptions> clientOptions = GetMockOptions();

            ESOverrideQueryService query = new ESOverrideQueryService(client, clientOptions, new NullLogger<ESOverrideQueryService>());

            // For this test, we don't really care that this returns anything, only that the intercepting connection
            // sets up the request correctly.
            await query.GetByPrettyUrlName(theName);

            Assert.Equal("/listingpagev1/ListingInfo/_search", esURI.AbsolutePath);
            Assert.Equal("application/json", esContentType);
            Assert.Equal(HttpMethod.POST, esMethod);
            Assert.Equal(expectedRequest, requestBody, new JTokenEqualityComparer());
        }

        public static IEnumerable<object[]> GetByPrettyUrlName_Scenarios = new[]
        {
            new object[] { new GetByPrettyUrlName_NoResults() },
            new object[] { new GetByPrettyUrlName_SingleResult() },
            new object[] { new GetByPrettyUrlName_MultipleResults() }
        };

        /// <summary>
        /// Test to verify handling when no results are returned.
        /// </summary>
        [Theory, MemberData(nameof(GetByPrettyUrlName_Scenarios))]
        public async void GetByPrettyUrlName_TestNoResultsFound(GetByPrettyUrlName_BaseScenario data)
        {
            ElasticsearchInterceptingConnection conn = new ElasticsearchInterceptingConnection();
            conn.RegisterRequestHandlerForType<Nest.SearchResponse<OverrideRecord>>((req, res) =>
            {
                res.Stream = TestingTools.GetStringAsStream(data.MockESResponse);
                res.StatusCode = 200;
            });

            // The URI does not matter, an InMemoryConnection never requests from the server.
            var pool = new SingleNodeConnectionPool(new Uri("http://localhost:9200"));

            var connectionSettings = new ConnectionSettings(pool, conn);
            IElasticClient client = new ElasticClient(connectionSettings);

            // Setup the mocked Options
            IOptions<ListingPageAPIOptions> clientOptions = GetMockOptions();

            ESOverrideQueryService query = new ESOverrideQueryService(client, clientOptions, new NullLogger<ESOverrideQueryService>());

            // The actual input doesn't matter for this test since the result is pre-determined.
            OverrideRecord result = await query.GetByPrettyUrlName("chicken");
            Assert.Equal(data.ExpectedData, result, new OverrideRecordComparer());
        }


        /// <summary>
        /// Simulates a "no results found" response from Elasticsearch so we
        /// have something for tests where we don't care about the response.
        /// </summary>
        private Stream MockEmptyResponse
        {
            get
            {
                string empty = @"
{
    ""took"": 223,
    ""timed_out"": false,
    ""_shards"": {
                ""total"": 1,
        ""successful"": 1,
        ""skipped"": 0,
        ""failed"": 0
    },
    ""hits"": {
                ""total"": 0,
        ""max_score"": null,
        ""hits"": []
    }
}";
                byte[] byteArray = Encoding.UTF8.GetBytes(empty);
                return new MemoryStream(byteArray);
            }
        }

        /// <summary>
        /// Mock Elasticsearch configuraiton options.
        /// </summary>
        private IOptions<ListingPageAPIOptions> GetMockOptions()
        {
            Mock<IOptions<ListingPageAPIOptions>> clientOptions = new Mock<IOptions<ListingPageAPIOptions>>();
            clientOptions
                .SetupGet(opt => opt.Value)
                .Returns(new ListingPageAPIOptions()
                {
                    ListingInfoAliasName = "listingpagev1"
                }
            );

            return clientOptions.Object;
        }

    }
}