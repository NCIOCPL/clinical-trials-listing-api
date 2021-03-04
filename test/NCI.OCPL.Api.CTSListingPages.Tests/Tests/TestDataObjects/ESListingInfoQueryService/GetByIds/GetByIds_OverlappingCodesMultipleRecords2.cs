namespace NCI.OCPL.Api.CTSListingPages.Tests
{
    /// <summary>
    /// Test for multiple records found, the second of which does not have all the c-codes
    /// from the requests.
    ///
    /// This is expected to return no records as ideally the service would never find multiple records
    /// matching the same c-codes and reacts to finding one missing by throwing out all the records
    /// because "all the records" should only be one in the first place.
    ///
    /// (See comments in ESListingInfoQueryService::GetByIds for more details.)
    /// </summary>
    class GetByIds_OverlappingCodesMultipleRecords2 : GetByIds_BaseScenario
    {
        // C8578 is present in the first record, missing from the second.
        public override string[] InputCCodes => new string[] { "C115270", "C8578", "C9092"};

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
            },
            {
                ""_index"": ""listinginfov1"",
                ""_type"": ""ListingInfo"",
                ""_id"": ""AXf-DxRKzojRwC-tFiLm"",
                ""_score"": null,
                ""_source"": {
                    ""concept_id"": [
                        ""C115270"",
                        ""C9092"",
                        ""C3017""
                    ],
                    ""name"": {
                        ""label"": ""Rhabdomyosarcoma"",
                        ""normalized"": ""rhabdomyosarcoma""
                    },
                    ""pretty_url_name"": ""rhabdomyosarcoma""
                },
                ""sort"": [
                    ""rhabdomyosarcoma""
                ]
            }
        ]
    }
}";

        public override ListingInfo[] ExpectedData => null;
    }
}
