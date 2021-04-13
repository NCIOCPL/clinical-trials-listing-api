namespace NCI.OCPL.Api.CTSListingPages.Tests
{
    /// <summary>
    /// Test for data issues where a pretty URL returns multiple documents.
    /// </summary>
    class GetByIds_MultipleResults : GetByIds_BaseScenario
    {
        public override string[] InputCCodes => new string[] { "C4872" };

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
                        ""C4872""
                    ],
                    ""name"": {
                        ""label"": ""Breast Cancer"",
                        ""normalized"": ""breast cancer""
                    },
                    ""pretty_url_name"": ""breast-cancer""
                }
            },
            {
                ""_index"": ""listinginfov1"",
                ""_type"": ""ListingInfo"",
                ""_id"": ""AXUtWHSZPZ7BGoClhnsQ"",
                ""_score"": 2.2335923,
                ""_source"": {
                    ""concept_id"": [
                        ""C4872""
                    ],
                    ""name"": {
                        ""label"": ""Breast Cancer"",
                        ""normalized"": ""breast cancer""
                    },
                    ""pretty_url_name"": ""breast-cancer""
                }
            }
        ]
    }
}";

        public override ListingInfo[] ExpectedData => new ListingInfo[]
        {
            new ListingInfo {
                ConceptId = new string[] { "C4872" },
                Name = new NameInfo
                {
                    Label = "Breast Cancer",
                    Normalized = "breast cancer"
                },
                PrettyUrlName = "breast-cancer"
            },
            new ListingInfo {
                ConceptId = new string[] { "C4872" },
                Name = new NameInfo
                {
                    Label = "Breast Cancer",
                    Normalized = "breast cancer"
                },
                PrettyUrlName = "breast-cancer"
            }
        };
    }
}
