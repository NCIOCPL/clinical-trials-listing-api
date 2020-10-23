using Moq;

namespace NCI.OCPL.Api.CTSListingPages.Tests
{
    class Get_SingleResult : Get_BaseScenario
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
                ""_index"": ""labelinformationv1"",
                ""_type"": ""LabelInformation"",
                ""_id"": ""AXUtWHSZPZ7BGoClhnsQ"",
                ""_score"": 2.2335923,
                ""_source"": {
                    ""pretty_url_name"": ""treatment"",
                    ""id_string"": ""treatment"",
                    ""label"": ""Treatment""
                }
            }
        ]
    }
}";

        public override LabelInformation ExpectedData => new LabelInformation
        {
            PrettyUrlName = "treatment",
            IdString = "treatment",
            Label = "Treatment"
        };

        public override string ErrorMessage => null;

        public override int ExpectedLoggerCalls => 0;
    }
}