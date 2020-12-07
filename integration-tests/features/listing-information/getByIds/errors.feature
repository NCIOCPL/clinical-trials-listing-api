Feature: Fail to retrieve listing information when searching for c-code(s) associated with multiple records.

    Background:
        * url apiHost

    Scenario Outline: Validate no result found when matching against c-code(s) returning multiple records.


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
