using System;
using System.Collections.Generic;

using Elasticsearch.Net;
using Microsoft.Extensions.Logging;
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
    public partial class ESListingInfoQueryServiceTest
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
        ""term"": { ""pretty_url_name"": { ""value"": ""recurrent-adult-brain"" } }
    }
}
");

            Uri esURI = null;
            string esContentType = String.Empty;
            HttpMethod esMethod = HttpMethod.DELETE; // Basically, something other than the expected value.

            JObject requestBody = null;

            ElasticsearchInterceptingConnection conn = new ElasticsearchInterceptingConnection();
            conn.RegisterRequestHandlerForType<Nest.SearchResponse<ListingInfo>>((req, res) =>
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

            ESListingInfoQueryService query = new ESListingInfoQueryService(client, clientOptions, new NullLogger<ESListingInfoQueryService>());

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
            conn.RegisterRequestHandlerForType<Nest.SearchResponse<ListingInfo>>((req, res) =>
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

            // Verify appropriate logging happens.
            Mock<ILogger<ESListingInfoQueryService>> _mockLogger = new Mock<ILogger<ESListingInfoQueryService>>();
            _mockLogger.Setup(log => log.Log(
                It.IsAny<Microsoft.Extensions.Logging.LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>())
            );

            ESListingInfoQueryService query = new ESListingInfoQueryService(client, clientOptions, _mockLogger.Object);

            // The actual input doesn't matter for this test since the result is pre-determined.
            ListingInfo result = await query.GetByPrettyUrlName("chicken");

            Assert.Equal(data.ExpectedData, result, new ListingInformationComparer());

            // Verify logging happens only once if there are multiple results and not at all if there are multiple.
            _mockLogger.Verify(
                x => x.Log(
                    It.Is<Microsoft.Extensions.Logging.LogLevel>(l => l == Microsoft.Extensions.Logging.LogLevel.Warning),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == $"Found multiple records for pretty URL name 'chicken'."),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
                ),
                Times.Exactly(data.ExpectedNumberOfLoggingCalls)
            );

        }

    }
}