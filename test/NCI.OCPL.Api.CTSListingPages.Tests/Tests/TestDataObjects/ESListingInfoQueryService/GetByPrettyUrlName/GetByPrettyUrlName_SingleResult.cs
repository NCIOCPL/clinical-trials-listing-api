namespace NCI.OCPL.Api.CTSListingPages.Tests
{
    class GetByPrettyUrlName_SingleResult : GetByPrettyUrlName_BaseScenario
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
                        ""C114967"",
                        ""C114964"",
                        ""C114972"",
                        ""C114963""
                    ],
                    ""name"": {
                        ""label"": ""Childhood Brain Cancer"",
                        ""normalized"": ""childhood brain cancer""
                    },
                    ""pretty_url_name"": ""childhood-brain-cancer""
                }
            }
        ]
    }
}";

        public override ListingInfo ExpectedData => new ListingInfo
        {
            ConceptId = new string[] {"C114967", "C114964", "C114972", "C114963" },
            Name = new NameInfo
            {
                Label = "Childhood Brain Cancer",
                Normalized = "childhood brain cancer"
            },
            PrettyUrlName = "childhood-brain-cancer"
        };

        public override int ExpectedNumberOfLoggingCalls => 0;
    }
}