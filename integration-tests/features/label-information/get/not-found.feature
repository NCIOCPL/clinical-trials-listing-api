Feature: Fail to retrieve trial type info by specifying a nonexistent name.

    Background:
        * url apiHost

    Scenario Outline: Validate no result found when matching against name.

        Given path 'trial-type', name
        When method get
        Then assert responseStatus == exStatus
        And match response == read( expected )

        Examples:
            | exStatus | name                      | expected                               |
            | 404      | chicken                   | not-found-chicken.json                 |
            | 404      | screening-diagnostic      | not-found-screening-diagnostic.json    |