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
            | exStatus  | code                                            | expected                     |
            | 400       | chicken                                         | errors-invalid-format.json   |
            | 400       | evil-name&quot;<script>alert(\"evil\")</script> | errors-invalid-format.json   |
            | 400       | Robert'); Drop table students;--                | errors-invalid-format.json   |
            | 404       | C0000                                           | errors-nonexistent-code.json |
            | 404       | ['C0000', 'C0001']                              | errors-nonexistent-code.json |


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
