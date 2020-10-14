Feature: Get override records by specifying the pretty URL name.

    Background:
        * url apiHost

    Scenario Outline: Validate the query results when matching against a pretty-url name.

        Given path 'override-name', prettyName
        When method get
        Then assert responseStatus == exStatus
        And match response == read( expected )

        Examples:
            | exStatus | prettyName                | expected                      |
            # Note: Multiple records with a single pretty-url name is a data error, and as the
            # test data is a snapshot of production, it's not practical to create a deliberate
            # test for a multiple record return.
            | 200      | adult-soft-tissue-sarcoma | name-match-exact-match.json   |
            | 404      | adult-soft                | name-match-partial-match.json |
            | 404      | chicken                   | name-match-not-a-match.json   |
