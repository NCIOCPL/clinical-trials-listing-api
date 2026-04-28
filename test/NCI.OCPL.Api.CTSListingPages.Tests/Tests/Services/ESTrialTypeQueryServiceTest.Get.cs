using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json.Nodes;

using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Moq;
using Xunit;

using NCI.OCPL.Api.Common;
using NCI.OCPL.Api.Common.Testing;
using NCI.OCPL.Api.CTSListingPages.Models;
using NCI.OCPL.Api.CTSListingPages.Services;

namespace NCI.OCPL.Api.CTSListingPages.Tests
{
    public partial class ETrialTypeQueryServiceTest
    {
        /// <summary>
        /// Test to verify that Elasticsearch requests are being assembled correctly.
        /// </summary>
        [Fact]
        public async void Get_TestRequestSetup()
        {
            const string theName = "supportive-care";
            JsonNode expectedRequest = JsonNode.Parse(
@"{
    ""query"": {
        ""bool"": {
            ""should"": [
                { ""term"": { ""pretty_url_name"": { ""value"": ""supportive-care"" } } },
                { ""term"": { ""id_string"":       { ""value"": ""supportive-care"" } } }
            ]
        }
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

            ESTrialTypeQueryService query = new ESTrialTypeQueryService(client, clientOptions, new NullLogger<ESTrialTypeQueryService>());

            // For this test, we don't really care that this returns anything, only that the intercepting connection
            // sets up the request correctly.
            await query.Get(theName);

            Assert.Equal("/trialtypeinfov1/_search", esURI.AbsolutePath);
            Assert.Equal(HttpMethod.POST, esMethod);
            Assert.True(JsonNode.DeepEquals(expectedRequest, requestBody));
        }

        /// <summary>
        /// Test failure to connect to ES or receiving an invalid response from ES.
        /// </summary>
        [Fact]
        public async void Get_TestInvalidResponse()
        {
            var connectionSettings = TestingElasticsearchClientSettingsFactory.Create("{}", 500);
            ElasticsearchClient client = new ElasticsearchClient(connectionSettings);

            // Setup the mocked Options
            IOptions<ListingPageAPIOptions> clientOptions = GetMockOptions();

            Mock<ILogger<ESTrialTypeQueryService>> _mockLogger = new Mock<ILogger<ESTrialTypeQueryService>>();

            _mockLogger.Setup(log => log.Log(
                It.IsAny<Microsoft.Extensions.Logging.LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>())
            );

            ESTrialTypeQueryService queryService = new ESTrialTypeQueryService(client, clientOptions, _mockLogger.Object);

            APIInternalException ex = await Assert.ThrowsAsync<APIInternalException>(() => queryService.Get("chicken"));

            // Verify the correct error message is thrown the correct number of times.
            _mockLogger.Verify(
                x => x.Log(
                    It.Is<Microsoft.Extensions.Logging.LogLevel>(l => l == Microsoft.Extensions.Logging.LogLevel.Error),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == "Invalid response when searching for pretty URL name or identifier 'chicken'."),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
                ),
                Times.Once
            );
        }

        public static IEnumerable<object[]> Get_Success_Scenarios = new[]
        {
            new object[] { new Get_NoResults() },
            new object[] { new Get_SingleResult() },
            new object[] { new Get_MultipleResults() }
        };

        /// <summary>
        /// Test to verify handling when a get request results in no errors (no results, single result, multiple results).
        /// </summary>
        [Theory, MemberData(nameof(Get_Success_Scenarios))]
        public async void Get_TestSuccess(Get_BaseScenario data)
        {
            var connectionSettings = TestingElasticsearchClientSettingsFactory.Create(data.MockESResponse, 200);
            ElasticsearchClient client = new ElasticsearchClient(connectionSettings);

            // Setup the mocked Options
            IOptions<ListingPageAPIOptions> clientOptions = GetMockOptions();

            Mock<ILogger<ESTrialTypeQueryService>> _mockLogger = new Mock<ILogger<ESTrialTypeQueryService>>();

            _mockLogger.Setup(log => log.Log(
                It.IsAny<Microsoft.Extensions.Logging.LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception, string>>() )
            );

            ESTrialTypeQueryService query = new ESTrialTypeQueryService(client, clientOptions, _mockLogger.Object);

            TrialTypeInfo result = await query.Get("treatment");

            // Verify that the logger logs a warning the expected number of times, with the expected error message.
            // For no results and one result, the logger logs zero warnings with no error message.
            // For multiple results, the logger logs one warning with the expected error message.
            _mockLogger.Verify(
                x => x.Log(
                    It.Is<Microsoft.Extensions.Logging.LogLevel>(l => l == Microsoft.Extensions.Logging.LogLevel.Warning),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString() == data.ErrorMessage),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
                ),
                Times.Exactly(data.ExpectedLoggerCalls)
            );

            Assert.Equal(data.ExpectedData, result, new TrialTypeComparer());
        }

        /// <summary>
        /// Simulates a "no results found" response from Elasticsearch so we
        /// have something for tests where we don't care about the response.
        /// </summary>
        private string MockEmptyResponseBody => @"
{
    ""took"" : 3,
    ""timed_out"" : false,
    ""_shards"" : {
        ""total"" : 1,
        ""successful"" : 1,
        ""skipped"" : 0,
        ""failed"" : 0
    },
    ""hits"" : {
        ""total"" : {
            ""value"" : 0,
            ""relation"" : ""eq""
        },
        ""max_score"" : null,
        ""hits"" : [ ]
    }
}";

        /// <summary>
        /// Mock Elasticsearch configuration options.
        /// </summary>
        private IOptions<ListingPageAPIOptions> GetMockOptions()
        {
            Mock<IOptions<ListingPageAPIOptions>> clientOptions = new Mock<IOptions<ListingPageAPIOptions>>();
            clientOptions
                .SetupGet(opt => opt.Value)
                .Returns(new ListingPageAPIOptions()
                {
                    TrialTypeInfoAliasName = "trialtypeinfov1"
                }
            );

            return clientOptions.Object;
        }

    }
}