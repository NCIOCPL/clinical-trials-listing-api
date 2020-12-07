namespace NCI.OCPL.Api.CTSListingPages.Tests
{
    /// <summary>
    /// Test for data issues where a pretty URL returns multiple documents.
    /// </summary>
    class GetByPrettyUrlName_MultipleResults : GetByPrettyUrlName_BaseScenario
    {
        public override string MockESResponse => @"
{
    ""took"": 20,
    ""timed_out"": false,
    ""_shards"": {
        ""total"": 1,
        ""successful"": 1,
        ""skipped"": 0,
        ""failed"": 0
    },
    ""hits"": {
        ""total"": 2,
        ""max_score"": 2.2335923,
        ""hits"": [
            {
                ""_index"": ""listinginfov1"",
                ""_type"": ""ListingInfo"",
                ""_id"": ""AXUtWHSZPZ7BGoClhnsQ"",
                ""_score"": 2.2335923,
                ""_source"": {
                    ""concept_id"": [
                        ""C123"",
                        ""C456"",
                        ""C789"",
                        ""C876""
                    ],
                    ""name"": {
                        ""label"": ""First item with this pretty url"",
                        ""normalized"": ""first item with  this pretty url""
                    },
                    ""pretty_url_name"": ""duplicated-pretty-url""
                }
            },
            {
                ""_index"": ""listinginfov1"",
                ""_type"": ""ListingInfo"",
                ""_id"": ""AXUtWHSZPZ7BGoClhnsQ"",
                ""_score"": 2.2335923,
                ""_source"": {
                    ""concept_id"": [
                        ""C123"",
                        ""C456"",
                        ""C789"",
                        ""C876""
                    ],
                    ""name"": {
                        ""label"": ""Second item with this pretty url"",
                        ""normalized"": ""second item with  this pretty url""
                    },
                    ""pretty_url_name"": ""duplicated-pretty-url""
                }
            }
        ]
    }
}";

        public override ListingInfo ExpectedData => new ListingInfo
        {
            ConceptId = new string[] {"C123", "C456", "C789", "C876" },
            Name = new NameInfo
            {
                Label = "First item with this pretty url",
                Normalized = "first item with  this pretty url"
            },
            PrettyUrlName = "duplicated-pretty-url"
        };

        public override int ExpectedNumberOfLoggingCalls => 1;
    }
}