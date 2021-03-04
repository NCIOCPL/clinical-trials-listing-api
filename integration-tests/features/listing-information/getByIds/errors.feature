Feature: Expected failures

    Background:
        * url apiHost

    Scenario Outline: Validate no result found when matching against c-code(s) returning multiple records.
        for code list: <code>, expect status <exStatus>

        Given path 'listing-information', 'get'
        And params { ccode: <code> }
        When method get
        Then assert responseStatus == exStatus
        And match response == read( expected )

        Examples:
            # Note: Multiple records for an ID or set of IDs should not happen but are possible.
            | exStatus  | code                | expected                                           |
            | 404       | chicken             | errors-invalid-code.json                           |
            | 404       | C0000               | errors-nonexistent-code.json                       |
            | 404       | ['C0000', 'C0001']  | errors-multiple-nonexistent-codes.json             |
            | 409       | C7707               | errors-multiple-soft-tissue-sarcoma.json           |
            | 409       | C7884               | errors-multiple-recurrent-brain-neoplasm.json      |


    Scenario Outline: Validate no result returned when request contains a c-code not found in the record.
        for code list: <code>, expect status <exStatus>

        Given path 'listing-information', 'get'
        And params { ccode: <code> }
        When method get
        Then assert responseStatus == exStatus
        And match response == read( expected )

        Examples:
            | exStatus  | code                                              | expected                                  |
            | 404       | ['C115332', 'C7705', 'C3359', 'C9130', 'C0001']   | errors-excess-code.json                   |
            | 404       | ['C115332', 'C7705', 'C0001']                     | errors-partially-overlapping-codes.json   |
