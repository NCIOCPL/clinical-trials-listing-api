Feature: Get trial type info by specifying the name.

    Background:
        * url apiHost

    Scenario Outline: Validate the query results when matching against name.

        Given path 'trial-type', name
        When method get
        Then assert responseStatus == exStatus
        And match response == read( expected )

        Examples:
            | exStatus | name                      | expected                                |
            | 200      | health-services-research  | name-match-pretty-url-name-match.json   |
            | 200      | health_services_research  | name-match-id-string-match.json         |
            | 200      | prevention                | name-match-both-match.json              |