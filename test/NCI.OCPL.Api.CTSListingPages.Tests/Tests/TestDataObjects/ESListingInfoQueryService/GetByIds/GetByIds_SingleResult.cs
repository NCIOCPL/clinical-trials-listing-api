namespace NCI.OCPL.Api.CTSListingPages.Tests
{
    class GetByIds_SingleResult : GetByIds_BaseScenario
    {
        public override string[] InputCCodes => new string[] { "C8578", "C115270" };

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
        ""total"": 1,
        ""max_score"": 2.2335923,
        ""hits"": [
            {
                ""_index"": ""listinginfov1"",
                ""_type"": ""ListingInfo"",
                ""_id"": ""AXUtWHSZPZ7BGoClhnsQ"",
                ""_score"": 2.2335923,
                ""_source"": {
                    ""type"": ""OverrideRecord"",
                    ""concept_id"": [
                        ""C115270"",
                        ""C8578"",
                        ""C9092"",
                        ""C3017""
                    ],
                    ""name"": {
                        ""label"": ""Ependymoma"",
                        ""normalized"": ""ependymoma""
                    },
                    ""pretty_url_name"": ""ependymoma""
                }
            }
        ]
    }
}";

        public override ListingInfo[] ExpectedData => new ListingInfo[] {
            new ListingInfo {
                ConceptId = new string[] { "C115270", "C8578", "C9092", "C3017" },
                Name = new NameInfo
                {
                    Label = "Ependymoma",
                    Normalized = "ependymoma"
                },
                PrettyUrlName = "ependymoma"
            }
        };
    }
}
