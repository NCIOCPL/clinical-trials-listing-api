Feature: Placeholder API Values method.

  Background:
    * url apiHost

  Scenario: get the list of values
    Given path 'api', 'Values',
    When method get
    Then status 200
    And match response == [ 'value1', 'value2' ]
