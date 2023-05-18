Feature: Healthcheck

    The system is able to report whether it is a healthy condition.

    Background:
        * url apiHost

    Scenario: The index has been loaded.

        Given path 'HealthCheck', 'status'
        When method get
        Then status 200
        And match response == 'alive!'