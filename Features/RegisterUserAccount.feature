Feature: Register user account

As a user
I want to sign up
So that I can access the application

@ignore-webclient @ignore-graphql
Scenario: The Sign Up page
  Given the landing page
  When I click on the "sign up" link
  Then I see the "sign up" page
  And I see the "username" field
  And I see the "password" field
  And I see the "confirm password" field
  And I see the "hint" field
  And I see the "sign up" button

@ignore
Scenario: Create account
  Given the sign up page
  And the "username" field filled with "username"
  And the "password" field filled with "passWORD$123"
  And the "confirm password" field filled with "passWORD$123"
  And the "hint" field filled with "account hint"
  And the "email" field filled with "test@email.com"
  When I click on the "sign up" button
  Then I see the "acounts" page
  And the current user is "username"

@ignore
Scenario: Error messages for mandatory input fields
  Given the sign up page
  When I click on the "sign up" button
  Then I see "A username is required" error message for the "username" field
  And I see "A password is required" error message for the "password" field
  And I see "A matching password is required" error message for the "confirm password" field
  And I see "A hint is required" error message for the "hint" field
  And I see "An email address is required" error message for the "email" field

@ignore
Scenario: Error messages for invalid email address
  Given the sign up page
  And the "username" field filled with "username"
  And the "password" field filled with "passWORD$123"
  And the "confirm password" field filled with "passWORD$123"
  And the "hint" field filled with "account hint"
  And the "email" field filled with "invalid"
  When I click on the "sign up" button
  Then I see "A valid email address is required" error message for the "email" field

@ignore
Scenario: Error messages for not matching passwords
  Given the sign up page
  And the "username" field filled with "username"
  And the "password" field filled with "passWORD$123"
  And the "confirm password" field filled with "something else"
  And the "hint" field filled with "account hint"
  And the "email" field filled with "test@email.com"
  When I click on the "sign up" button
  Then I see "Passwords do not match" error message for the "password" field

@ignore
Scenario: Duplicate account
  Given the sign up page
  And the "username" field filled with "username"
  And the "password" field filled with "passWORD$123"
  And the "confirm password" field filled with "passWORD$123"
  And the "hint" field filled with "account hint"
  And the "email" field filled with "test@email.com"
  But a user with "USERNAME" username and "different@email.com" email already exists
  When I click on the "sign up" button
  Then I see "The username is unavailable" error message for the "username" field

@ignore
Scenario: Duplicate email
  Given the sign up page
  And the "username" field filled with "username"
  And the "password" field filled with "passWORD$123"
  And the "confirm password" field filled with "passWORD$123"
  And the "hint" field filled with "account hint"
  And the "email" field filled with "test@email.com"
  But a user with "different username" username and "TEST@email.com" email already exists
  When I click on the "sign up" button
  Then I see "The email is unavailable" error message for the "email" field

@ignore
Scenario: Field character limit
  Given the sign up page
  And the "username" field filled with 251 characters
  And the "password" field filled with 251 characters
  And the "confirm password" field filled with 251 characters
  And the "hint" field filled with 251 characters
  When I click on the "sign up" button
  Then I see "The username can be at most 250 characters." error message for the "username" field
  And I see "The password can be at most 250 characters." error message for the "password" field
  And I see "The hint can be at most 250 characters." error message for the "hint" field

# Scenario: Invalid Characters in Input Fields
# Scenario: Registration Fails Due to Server Error

@ignore
Scenario: Password strength
  Given the sign up page
  And the "username" field filled with "username"
  And the "password" field filled with "pass"
  And the "confirm password" field filled with "pass"
  And the "hint" field filled with "account hint"
  When I click on the "sign up" button
  And I see "The password must be strong, at least 8 characters long containing both lowercase and uppercase letters alongside at least one numeric and special character" error message for the "password" field

@ignore
Scenario: Cancel button
  Given the sign up page
  When I click on the "cancel" button
  Then I see the "Login" page

@ignore
Scenario: Cancelling account creation
  Given the sign up page
  And the "username" field filled with "username"
  Then I see the "Login" page