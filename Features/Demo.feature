Feature: demo

  As a user
  I want to have features clearly defined
  So that I know what this app should be doing

  Scenario: Check if there's a button
    Given the landing page
    When I look at the page
    Then I see a "Use application locally" link
