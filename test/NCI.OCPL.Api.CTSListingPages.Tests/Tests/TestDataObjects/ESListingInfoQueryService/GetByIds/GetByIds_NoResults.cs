namespace NCI.OCPL.Api.CTSListingPages.Tests
{
    class GetByIds_NoResults : GetByIds_BaseScenario
    {
        public override string[] InputCCodes => new string[] {"C1234"};

        public override string MockESResponse => @"
{
    ""took"": 223,
    ""timed_out"": false,
    ""_shards"": {
                ""total"": 1,
        ""successful"": 1,
        ""skipped"": 0,
        ""failed"": 0
    },
    ""hits"": {
                ""total"": 0,
        ""max_score"": null,
        ""hits"": []
    }
}";

        public override ListingInfo[] ExpectedData => null;
    }
}
