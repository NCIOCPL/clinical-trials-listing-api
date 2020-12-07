Feature: Get listing information records by specifying their c-code(s).

    Background:
        * url apiHost

    Scenario Outline: Validate the query results when matching against c-code(s).


        Given path 'listing-information', 'get'
        And params { ccode: <code> }
        When method get
        Then status 200
        And match response == read( expected )

        Examples:
            | code                                           | expected                                                |
            | ['C114967', 'C114964', 'C114972', 'C114963']   | id-found-all-codes-childhood-brain-cancer.json          |
            | ['C114964', 'C114963']                         | id-found-two-missing-codes-childhood-brain-cancer.json  |
            | C114972                                        | id-found-single-code-childhood-brain-cancer.json        |
            | ['C3359', 'C7705', 'C9130', 'C115332']         | id-found-all-codes-rhabdomyosarcoma.json                |
            | C3359                                          | id-found-single-code-rhabdomyosarcoma.json              |
            | ['C9130', 'C115332', 'C7705']                  | id-found-single-missing-code-rhabdomyosarcoma.json      |
