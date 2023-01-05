Feature: Pretty url names only allow letters (upper and lower case), numbers, and hyphens

    Background:
        * url apiHost

    Scenario Outline: Successful listing info lookup with a valid pretty-url name.
        for pretty name: '<prettyName>'

        Given path 'listing-information', prettyName
        When method get
        Then status 200

        Examples:
            | prettyName          |
            | soft-tissue-sarcoma |
            | a1-hydronephrosis   |
            | 125iudr             |
            | abacavir            |


    Scenario Outline: Disallowed listing info lookup with invalid pretty-url name.
        for pretty name: '<prettyName>'

        Given path 'listing-information', prettyName
        When method get
        Then status 400

        Examples:
            | prettyName          |
            | ' '                 |
            | ''                  |
            | a1 hydronephrosis   |
            | a1_hydronephrosis   |
            | a1+hydronephrosis   |
            | a1%20hydronephrosis |
            | expr${IFS}26        |
            | 🐔                  |
