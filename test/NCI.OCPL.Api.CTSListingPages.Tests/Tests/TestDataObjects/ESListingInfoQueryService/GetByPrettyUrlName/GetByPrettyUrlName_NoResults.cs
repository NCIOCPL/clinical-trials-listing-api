namespace NCI.OCPL.Api.CTSListingPages.Tests
{
    class GetByPrettyUrlName_NoResults : GetByPrettyUrlName_BaseScenario
    {
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

        public override ListingInfo ExpectedData => null;

        public override int ExpectedNumberOfLoggingCalls => 0;
    }
}