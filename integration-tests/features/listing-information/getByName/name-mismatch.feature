Feature: Fail to retrieve records when an invalid pretty URL name is specified.

    Background:
        * url apiHost

    Scenario Outline: Validate the query results when matching against a pretty-url name.
        for pretty name: '<prettyName>'

        Given path 'listing-information', prettyName
        When method get
        Then status 404
        And match response == read( expected )

        Examples:
            | prettyName                | expected                      |
            | adult-soft                | name-mismatch-partial-match.json |
            | chicken                   | name-mismatch-not-a-match.json   |
