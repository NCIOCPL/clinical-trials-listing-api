Feature: Get listing information records by specifying their pretty URL name.

    Background:
        * url apiHost

    Scenario Outline: Validate the query results when matching against a pretty-url name.

        Given path 'listing-information', prettyName
        When method get
        Then status 200
        And match response == read( expected )

        Examples:
            | prettyName                | expected                              |
            # Note: Multiple records with a single pretty-url name is a data error, and as the
            # test data is a snapshot of production, it's not practical to create a deliberate
            # test for a multiple record return.
            | soft-tissue-sarcoma       | name-match-record.json                |
