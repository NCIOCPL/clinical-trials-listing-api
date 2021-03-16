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

using NCI.OCPL.Api.Common;
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
        public async void GetByIds_TestRequestSetup()
        {
            string[] codes = new string[] { "C115270", "C8578", "C9092", "C3017" };
            JObject expectedRequest = JObject.Parse(
@"{
    ""query"": {
        ""terms"": { ""concept_id"": [ ""C115270"", ""C8578"", ""C9092"", ""C3017"" ] }
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
                esContentType = req.RequestMimeType;
                esMethod = req.Method;
                requestBody = conn.GetRequestPost(req);
            });

            // The URI does not matter, an intercepting connector never requests from the server.
            var pool = new SingleNodeConnectionPool(new Uri("http://localhost:9200"));

            var connectionSettings = new ConnectionSettings(pool, conn);
            IElasticClient client = new ElasticClient(connectionSettings);

            // Setup the mocked Options
            IOptions<ListingPageAPIOptions> clientOptions = GetMockOptions();

            ESListingInfoQueryService query = new ESListingInfoQueryService(client, clientOptions, new NullLogger<ESListingInfoQueryService>());

            // For this test, we don't really care that this returns anything, only that the intercepting connection
            // sets up the request correctly.
            await query.GetByIds(codes);

            Assert.Equal("/listingpagev1/_search", esURI.AbsolutePath);
            Assert.Equal("application/json", esContentType);
            Assert.Equal(HttpMethod.POST, esMethod);
            Assert.Equal(expectedRequest, requestBody, new JTokenEqualityComparer());
        }

        /// <summary>
        /// Test failure to connect to and retrieve response from Elasticsearch.
        /// </summary>
        [Fact]
        public async void GetByIds_TestAPIConnectionFailure()
        {
            ElasticsearchInterceptingConnection conn = new ElasticsearchInterceptingConnection();
            conn.RegisterRequestHandlerForType<Nest.SearchResponse<ListingInfo>>((req, res) =>
            {
                throw new Exception();
            });

            // While this has a URI, it does not matter, an intercepting connector never requests
            // from the server.
            var pool = new SingleNodeConnectionPool(new Uri("http://localhost:9200"));

            var connectionSettings = new ConnectionSettings(pool, conn);
            IElasticClient client = new ElasticClient(connectionSettings);

            // Setup the mocked Options
            IOptions<ListingPageAPIOptions> clientOptions = GetMockOptions();

            Mock<ILogger<ESListingInfoQueryService>> _mockLogger = new Mock<ILogger<ESListingInfoQueryService>>();

            _mockLogger.Setup(log => log.Log(
                It.IsAny<Microsoft.Extensions.Logging.LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>())
            );

            ESListingInfoQueryService queryService = new ESListingInfoQueryService(client, clientOptions, _mockLogger.Object);

            APIInternalException ex = await Assert.ThrowsAsync<APIInternalException>(() => queryService.GetByIds(new string[] { "chicken" }));

            // Verify the correct error message is thrown the correct number of times.
            _mockLogger.Verify(
                x => x.Log(
                    It.Is<Microsoft.Extensions.Logging.LogLevel>(l => l == Microsoft.Extensions.Logging.LogLevel.Error),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "Error searching index: 'listingpagev1'."),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
                ),
                Times.Once
            );
        }

        /// <summary>
        /// Test receiving an invalid response from ES.
        /// </summary>
        [Fact]
        public async void GetByIds_TestInvalidResponse()
        {
            ElasticsearchInterceptingConnection conn = new ElasticsearchInterceptingConnection();
            conn.RegisterRequestHandlerForType<Nest.SearchResponse<ListingInfo>>((req, res) =>
            {

            });

            // While this has a URI, it does not matter, an intercepting connection never requests
            // from the server.
            var pool = new SingleNodeConnectionPool(new Uri("http://localhost:9200"));

            var connectionSettings = new ConnectionSettings(pool, conn);
            IElasticClient client = new ElasticClient(connectionSettings);

            // Setup the mocked Options
            IOptions<ListingPageAPIOptions> clientOptions = GetMockOptions();

            Mock<ILogger<ESListingInfoQueryService>> _mockLogger = new Mock<ILogger<ESListingInfoQueryService>>();

            _mockLogger.Setup(log => log.Log(
                It.IsAny<Microsoft.Extensions.Logging.LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>())
            );

            ESListingInfoQueryService queryService = new ESListingInfoQueryService(client, clientOptions, _mockLogger.Object);

            APIInternalException ex = await Assert.ThrowsAsync<APIInternalException>(() => queryService.GetByIds(new string[] { "chicken" }));

            // Verify the correct error message is thrown the correct number of times.
            _mockLogger.Verify(
                x => x.Log(
                    It.Is<Microsoft.Extensions.Logging.LogLevel>(l => l == Microsoft.Extensions.Logging.LogLevel.Error),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "Invalid response when searching for c-code(s) 'chicken'."),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
                ),
                Times.Once
            );
        }

        public static IEnumerable<object[]> GetByIds_Scenarios = new[]
        {
            new object[] { new GetByIds_NoResults() },
            new object[] { new GetByIds_SingleResult() },
            new object[] { new GetByIds_MultipleResults() },
            new object[] { new GetByIds_ExcessCodes() },
            new object[] { new GetByIds_OverlappingCodes() },
            new object[] { new GetByIds_OverlappingCodesMultipleRecords() },
            new object[] { new GetByIds_OverlappingCodesMultipleRecords2() },
        };

        /// <summary>
        /// Test to verify handling when calls are successful.
        /// </summary>
        [Theory, MemberData(nameof(GetByIds_Scenarios))]
        public async void GetByIds_TestValidResponse(GetByIds_BaseScenario data)
        {
            ElasticsearchInterceptingConnection conn = new ElasticsearchInterceptingConnection();
            conn.RegisterRequestHandlerForType<Nest.SearchResponse<ListingInfo>>((req, res) =>
            {
                res.Stream = TestingTools.GetStringAsStream(data.MockESResponse);
                res.StatusCode = 200;
            });

            // The URI does not matter, an intercepting connector never requests from the server.
            var pool = new SingleNodeConnectionPool(new Uri("http://localhost:9200"));

            var connectionSettings = new ConnectionSettings(pool, conn);
            IElasticClient client = new ElasticClient(connectionSettings);

            // Setup the mocked Options
            IOptions<ListingPageAPIOptions> clientOptions = GetMockOptions();

            ESListingInfoQueryService query = new ESListingInfoQueryService(client, clientOptions, new NullLogger<ESListingInfoQueryService>());

            ListingInfo[] result = await query.GetByIds(data.InputCCodes);

            Assert.Equal(data.ExpectedData, result, new ListingInformationComparer());
        }
    }
}
