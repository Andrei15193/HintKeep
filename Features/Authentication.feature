Feature: Authentication
As a user
I want to be able to log in
So that I can use the app

  @ignore-webclient
  Scenario: Login page
    Given the landing page
    Then I am on the "login" page
    And I have the "username" field
    And I have the "password" field
    And I have the "login" button
    And I have the "login with localDB" button
    And I have the "sign up" link

  @ignore-webclient
  Scenario: Login with missing account
    Given the landing page
    When I enter "test" in the "username" field
    And I enter "pa$$w0rd123" in the "password" field
    And I press the login button
    Then I have "Wrong credentials. Please try again or follow the password recovery steps" error message for the "username" field

  @ignore-webclient
  Scenario: Login with existing account and wrong credentials
    Given the landing page
    And a user with "test" username and "PA$$w0rd123" password
    When I enter "test" in the "username" field
    And I enter "wrong password" in the "password" field
    And I press the login button
    Then I have "Wrong credentials. Please try again or follow the password recovery steps" error message for the "username" field

  @ignore-webclient
  Scenario: Login with existing account and matching credentials
    Given the landing page
    And a user with "test" username and "PA$$w0rd123" password
    When I enter "test" in the "username" field
    And I enter "PA$$w0rd123" in the "password" field
    And I press the login button
    Then I am on the "accounts" page
