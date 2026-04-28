using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Nodes;

using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging.Testing;
using Moq;
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
        public async void GetByPrettyUrlName_TestRequestSetup()
        {
            const string theName = "recurrent-adult-brain";
            JsonNode expectedRequest = JsonNode.Parse(
@"{
    ""query"": {
        ""term"": { ""pretty_url_name"": { ""value"": ""recurrent-adult-brain"" } }
    },
    ""size"": 2
}
");

            Uri esURI = null;
            HttpMethod esMethod = HttpMethod.DELETE; // Basically, something other than the expected value.

            JsonNode requestBody = null;

            var connectionSettings = TestingElasticsearchClientSettingsFactory.Create(MockEmptyResponseBody, 200, details =>
            {
                esURI = details.Uri;
                esMethod = details.HttpMethod;
                requestBody = JsonNode.Parse(Encoding.UTF8.GetString(details.RequestBodyInBytes));
            });
            ElasticsearchClient client = new ElasticsearchClient(connectionSettings);

            // Setup the mocked Options
            IOptions<ListingPageAPIOptions> clientOptions = GetMockOptions();

            ESListingInfoQueryService query = new ESListingInfoQueryService(client, clientOptions, new NullLogger<ESListingInfoQueryService>());

            // For this test, we don't really care that this returns anything, only that the intercepting connection
            // sets up the request correctly.
            await query.GetByPrettyUrlName(theName);

            Assert.Equal("/listingpagev1/_search", esURI.AbsolutePath);
            Assert.Equal(HttpMethod.POST, esMethod);
            Assert.True(JsonNode.DeepEquals(expectedRequest, requestBody));
        }

        /// <summary>
        /// Test failure to connect to and retrieve response from API.
        /// </summary>
        [Fact]
        public async void GetByPrettyUrlName_TestAPIConnectionFailure()
        {
            var requestInvoker = new DynamicInMemoryConnection((data, json) => throw new Exception("connection failed"));
            var connectionSettings = TestingElasticsearchClientSettingsFactory.Create(requestInvoker);
            ElasticsearchClient client = new ElasticsearchClient(connectionSettings);

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

            APIInternalException ex = await Assert.ThrowsAsync<APIInternalException>(() => queryService.GetByPrettyUrlName("chicken"));

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
        public async void GetByPrettyUrlName_TestInvalidResponse()
        {
            var connectionSettings = TestingElasticsearchClientSettingsFactory.Create("{}", 500);
            ElasticsearchClient client = new ElasticsearchClient(connectionSettings);

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

            APIInternalException ex = await Assert.ThrowsAsync<APIInternalException>(() => queryService.GetByPrettyUrlName("chicken"));

            // Verify the correct error message is thrown the correct number of times.
            _mockLogger.Verify(
                x => x.Log(
                    It.Is<Microsoft.Extensions.Logging.LogLevel>(l => l == Microsoft.Extensions.Logging.LogLevel.Error),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "Invalid response when searching for pretty URL name 'chicken'."),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
                ),
                Times.Once
            );
        }

        public static IEnumerable<object[]> GetByPrettyUrlName_Scenarios = new[]
        {
            new object[] { new GetByPrettyUrlName_NoResults() },
            new object[] { new GetByPrettyUrlName_SingleResult() },
            new object[] { new GetByPrettyUrlName_MultipleResults() }
        };

        /// <summary>
        /// Test to verify handling when calls are successful.
        /// </summary>
        [Theory, MemberData(nameof(GetByPrettyUrlName_Scenarios))]
        public async void GetByPrettyUrlName_TestValidResponse(GetByPrettyUrlName_BaseScenario data)
        {
            var connectionSettings = TestingElasticsearchClientSettingsFactory.Create(data.MockESResponse, 200);
            ElasticsearchClient client = new ElasticsearchClient(connectionSettings);

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