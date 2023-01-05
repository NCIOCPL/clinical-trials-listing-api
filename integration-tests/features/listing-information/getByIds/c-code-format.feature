Feature: c-codes must match the pattern [cC][0-9]+

    Background:
        * url apiHost

    Scenario Outline: Successful listing info lookup with a valid c-code
        for code list: '<code>'

        Given path 'listing-information', 'get'
        And params { ccode: <code> }
        When method get
        Then status 200

        Examples:
            # Verify retrieving with a varying number of c-codes
            | code                                   |
            # Uppercase
            | ['C7707', 'C7715', 'C9306', 'C115292'] |
            | ['C7715', 'C9306']                     |
            | 'C115292'                              |
            # Lowercase/mixed-in
            | ['c7707', 'C7715', 'c9306', 'C115292'] |
            | ['C7715', 'c9306']                     |
            | 'c115292'                              |


    Scenario Outline: Disallowed listing info lookup with an invalid c-code
        for code list: '<code>'

        Given path 'listing-information', 'get'
        And params { ccode: <code> }
        When method get
        Then status 400

        Examples:
            | code                                                |
            | chicken                                             |
            | null                                                |
            | ''                                                  |
            | ' '                                                 |
            | '🐔'                                                |
            | 'C 115292'                                          |
            | 'D115292'                                           |
            | 'C-115292'                                          |
            | ['C7707', ' ', 'C9306', 'C115292']                  |
            | ['C7707', 'C9306', 'C115292', ' ']                  |
            | ['C7715', NULL]                                     |
            | ['evil-name&quot;<script>alert(\"evil\")</script>'] |
            | "Robert'); Drop table students;--"                  |
