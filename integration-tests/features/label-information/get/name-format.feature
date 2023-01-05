Feature: Pretty url names only allow letters (upper and lower case), numbers, and hyphens

    Background:
        * url apiHost


    Scenario Outline: Successful label info lookup with valid name inputs.
        for name: '<name>'

        Given path 'trial-type', name
        When method get
        Then status 200

        Examples:
            | name                     |
            | health-services-research |
            | health_services_research |
            | prevention               |


    Scenario Outline: Disallowed label info lookup with invalid name strings.
        for name: '<name>'

        Given path 'trial-type', name
        When method get
        Then status 400

        Examples:
            | name                                              |
            | ' '                                               |
            | ''                                                |
            | '🐔'                                              |
            | health services research                          |
            | basic science                                     |
            | basic+science                                     |
            | basic%20science                                   |
            | expr${IFS}26                                      |
            | evil-name%26quot%3B%3Cscript%3Ealert%28%255C%22evil%255C%22%29%3C%2Fscript%3E |
            | %22Robert%27%29%3B%20Drop%20table%20students%3B--%22                          |
