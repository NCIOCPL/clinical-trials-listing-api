using Moq;

namespace NCI.OCPL.Api.CTSListingPages.Tests
{
    class Get_MultipleResults : Get_BaseScenario
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
                ""_index"": ""trialtypeinfov1"",
                ""_type"": ""TrialTypeInfo"",
                ""_id"": ""AXUtWHSZPZ7BGoClhnsQ"",
                ""_score"": 2.2335923,
                ""_source"": {
                    ""pretty_url_name"": ""treatment"",
                    ""id_string"": ""treatment"",
                    ""label"": ""Treatment""
                }
            },
            {
                ""_index"": ""trialtypeinfov1"",
                ""_type"": ""TrialTypeInfo"",
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

        public override TrialTypeInfo ExpectedData => new TrialTypeInfo
        {
            PrettyUrlName = "treatment",
            IdString = "treatment",
            Label = "Treatment"
        };

        public override string ErrorMessage => "Found multiple records for pretty URL name or identifier 'treatment'.";

        public override int ExpectedLoggerCalls => 1;
    }
}