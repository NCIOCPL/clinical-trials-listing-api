Feature: Get listing information records by specifying their c-code(s).

    Background:
        * url apiHost

    Scenario Outline: Validate the query results when matching against c-code(s).
        for code list: '<code>'

        Given path 'listing-information', 'get'
        And params { ccode: <code> }
        When method get
        Then status 200
        And match response == read( expected )

        Examples:
            # Verify retrieving the same record with a varying number of c-codes
            | code                                          | expected                                                |
            | ['C7707', 'C7715', 'C9306', 'C115292']        | id-found-soft-tissue-sarcoma.json  |
            | ['C7715', 'C9306']                            | id-found-soft-tissue-sarcoma.json  |
            | 'C115292'                                     | id-found-soft-tissue-sarcoma.json  |
            | ['C3359', 'C7705', 'C9130']                   | id-found-rhabdomyosarcoma.json     |
            | C3359                                         | id-found-rhabdomyosarcoma.json     |
            | ['C7705', 'C9130']                            | id-found-rhabdomyosarcoma.json     |
