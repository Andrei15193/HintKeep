Feature: User sign-up

As a user
I want to sign up
So that I can access the application

Scenario: The Sign Up Page
  Given the landing page
  When I click on the "sign up" button
  Then I can see the "sign up" page
  And I can see the "username" field
  And I can see the "password" field
  And I can see the "hint" field
  And I can see the "sign up" button

Scenario: Create Account
  Given the sign up page
  And the "username" fields filled with "username"
  And the "password" fields filled with "passWORD$123"
  And the "hint" fields filled with "account hint"
  And the "email" fields filled with "test@email.com"
  When I click on the "sign up" button
  Then I can see the "acounts" page
  And the current user is "username"

Scenario: Error messages for mandatory input fields
  Given the sign up page
  When I click on the "sign up" button
  Then I can see "A username is required." error message for the "username" field
  And I can see "A password is required." error message for the "password" field
  And I can see "A hint is required." error message for the "hint" field
  And I can see "An email address is required." error message for the "email" field

@ignore
Scenario: Duplicate Account
  Given the sign up page
  And the "username" fields filled with "username"
  And the "password" fields filled with "passWORD$123"
  And the "hint" fields filled with "account hint"
  But a user with "username" username already exists
  When I click on the "sign up" button
  Then I can see "The username is unavailable" error message for the "username" field

@ignore
Scenario: Field Character Limit
  Given the sign up page
  And the "username" fields filled with 251 characters
  And the "password" fields filled with 251 characters
  And the "hint" fields filled with 251 characters
  When I click on the "sign up" button
  Then I can see "The username must be at most 250 characters" error message for the "username" field
  And I can see "The password must be at most 250 characters" error message for the "password" field
  And I can see "The hint must be at most 250 characters" error message for the "hint" field

@ignore
Scenario: Password Strength
  Given the sign up page
  And the "username" fields filled with "username"
  And the "password" fields filled with "pass"
  And the "hint" fields filled with "account hint"
  When I click on the "sign up" button
  And I can see "The password must be strong, at least 8 characters long containing both lowercase and uppercase letters alongside at least one numeric and special character." error message for the "password" field

@ignore
Scenario: Cancel Button
  Given the sign up page
  When I click on the "cancel" button
  Then I can see the landing page

@ignore
Scenario: Cancelling Creating an Account
  Given the sign up page
  And the "username" fields filled with "username"
  Then I can see the landing page