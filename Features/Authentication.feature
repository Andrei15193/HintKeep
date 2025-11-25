Feature: Authentication

  As a user
  I want to be able to log in
  So that I can use the app

  Scenario: Login with missing account
    Given the landing page
    And I click on "Use application locally"
    And I see "HintKeep - Login"
    When I enter "test" for "Username"
    And I enter "pa$$w0rd123" for "Password"
    And I click on 'Login'
    Then I see the "Wrong credentials. Try again or follow the password recovery steps." error message for "Username"