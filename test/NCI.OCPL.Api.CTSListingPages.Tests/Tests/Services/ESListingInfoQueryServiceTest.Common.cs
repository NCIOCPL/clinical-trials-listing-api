using System.IO;
using System.Text;

using Microsoft.Extensions.Options;
using Moq;

using NCI.OCPL.Api.CTSListingPages.Models;

namespace NCI.OCPL.Api.CTSListingPages.Tests
{
    public partial class ESListingInfoQueryServiceTest
    {
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
                    ListingInfoAliasName = "listingpagev1"
                }
            );

            return clientOptions.Object;
        }

    }
}