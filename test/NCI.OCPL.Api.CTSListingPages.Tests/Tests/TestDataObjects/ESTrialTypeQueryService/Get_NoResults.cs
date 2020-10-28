using Moq;

namespace NCI.OCPL.Api.CTSListingPages.Tests
{
    class Get_NoResults : Get_BaseScenario
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

        public override TrialTypeInfo ExpectedData => null;

        public override string ErrorMessage => null;

        public override int ExpectedLoggerCalls => 0;
    }
}