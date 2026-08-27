Feature: Register user account
As a user
I want to sign up
So that I can access the application

  @ignore-webclient
  Scenario: The Sign Up page
    Given the landing page
    When I press the "sign up" link
    Then I am on the "sign up" page
    And I have the "username" field
    And I have the "password" field
    And I have the "confirm password" field
    And I have the "hint" field
    And I have the "sign up" button

  @ignore-webclient
  Scenario: Create account
    Given the sign up page
    When I enter "username" in the "username" field
    And I enter "passWORD$123" in the "password" field
    And I enter "passWORD$123" in the "confirm password" field
    And I enter "account hint" in the "hint" field
    And I enter "test@email.com" in the "email" field
    And I press the sign up button
    Then I am on the "accounts" page
    And the current user is "username"

  @ignore-webclient
  Scenario: Error messages for mandatory input fields
    Given the sign up page
    When I press the "sign up" button
    Then I have "A username is required" error message for the "username" field
    And I have "A password is required" error message for the "password" field
    And I have "A matching password is required" error message for the "confirm password" field
    And I have "A hint is required" error message for the "hint" field
    And I have "An email address is required" error message for the "email" field

  @ignore-webclient
  Scenario: Error messages for invalid email address
    Given the sign up page
    When I enter "username" in the "username" field
    And I enter "passWORD$123" in the "password" field
    And I enter "passWORD$123" in the "confirm password" field
    And I enter "account hint" in the "hint" field
    And I enter "invalid" in the "email" field
    And I press the sign up button
    Then I have "A valid email address is required" error message for the "email" field

  @ignore-webclient
  Scenario: Error messages for not matching passwords
    Given the sign up page
    When I enter "username" in the "username" field
    And I enter "passWORD$123" in the "password" field
    And I enter "somenthing else" in the "confirm password" field
    And I enter "account hint" in the "hint" field
    And I enter "test@email.com" in the "email" field
    And I press the sign up button
    Then I have "Passwords do not match" error message for the "password" field

  @ignore-webclient
  Scenario: Duplicate account
    Given the sign up page
    And a user with "USERNAME" username and "different@email.com" email
    When I enter "username" in the "username" field
    And I enter "passWORD$123" in the "password" field
    And I enter "passWORD$123" in the "confirm password" field
    And I enter "account hint" in the "hint" field
    And I enter "test@email.com" in the "email" field
    And I press the sign up button
    Then I have "The username is unavailable" error message for the "username" field

  @ignore-webclient
  Scenario: Duplicate email
    Given the sign up page
    And a user with "different username" username and "TEST@email.com" email
    When I enter "username" in the "username" field
    And I enter "passWORD$123" in the "password" field
    And I enter "passWORD$123" in the "confirm password" field
    And I enter "account hint" in the "hint" field
    And I enter "test@email.com" in the "email" field
    And I press the sign up button
    Then I have "The email address is unavailable" error message for the "email" field

  @ignore-webclient
  Scenario: Field character limit
    Given the sign up page
    When I enter 251 characters in the "username" field
    And I enter 251 characters in the "password" field
    And I enter 251 characters in the "confirm password" field
    And I enter 251 characters in the "hint" field
    And I enter 251 character email address in the "email" field
    And I press the sign up button
    Then I have "The username can be at most 250 characters" error message for the "username" field
    And I have "The password can be at most 250 characters" error message for the "password" field
    And I have "The hint can be at most 250 characters" error message for the "hint" field
    And I have "The email address can be at most 250 characters" error message for the "email" field

# Scenario: Invalid Characters in Input Fields
# Scenario: Registration Fails Due to Server Error

  @ignore-webclient
  Scenario Outline: Password strength
    Given the sign up page
    When I enter "username" in the "username" field
    And I enter "<password>" in the "password" field
    And I enter "<password>" in the "confirm password" field
    And I enter "account hint" in the "hint" field
    And I enter "test@email.com" in the "email" field
    And I press the sign up button
    Then I have "The password must be strong, at least 8 characters long containing both lowercase and uppercase letters alongside at least one numeric and special character" error message for the "password" field

    Examples:
      | password                      |
      | pass                          |
      | PASS                          |
      | PaSs                          |
      | passwordWithManyCharacters    |
      | passwordWith24Characters      |
      | @@1234567890!@#$%             |
      | still_not_strong?\\/\\/hat?11 |

  @ignore-webclient
  Scenario: Cancel button
    Given the sign up page
    When I press the "cancel" link
    Then I am on the "Login" page

  @ignore-webclient
  Scenario: Cancelling account creation
    Given the sign up page
    When I enter "username" in the "username" field
    And I press the "cancel" link
    Then I am on the "Login" page
