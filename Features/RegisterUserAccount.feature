Feature: Register user account

As a user
I want to sign up
So that I can access the application

Scenario: The Sign Up Page
  Given the landing page
  When I click on the "sign up" link
  Then I see the "sign up" page
  And I see the "username" field
  And I see the "password" field
  # -> confirm password? so another field for it.
  And I see the "hint" field
  And I see the "sign up" button

# Scenario: Password and Confirmation Do Not Match - in case you add confirm pass field

@ignore
Scenario: Create Account
  Given the sign up page
  And the "username" field filled with "username"
  And the "password" field filled with "passWORD$123"
  # -> confirm password? so another field for it.
  And the "hint" field filled with "account hint"
  And the "email" field filled with "test@email.com"
  When I click on the "sign up" button
  Then I see the "acounts" page
  And the current user is "username"

@ignore
Scenario: Error messages for mandatory input fields
  Given the sign up page
  When I click on the "sign up" button
  Then I see "A username is required." error message for the "username" field
  And I see "A password is required." error message for the "password" field
  And I see "A hint is required." error message for the "hint" field
  And I see "An email address is required." error message for the "email" field

@ignore
Scenario: Error messages for invalid email address
  Given the sign up page
  And the "username" field filled with "username"
  And the "password" field filled with "passWORD$123"
  And the "hint" field filled with "account hint"
  And the "email" field filled with "invalid"
  When I click on the "sign up" button
  And I see "A valid email address is required." error message for the "email" field

@ignore
Scenario: Duplicate Account
  Given the sign up page
  And the "username" field filled with "username"
  And the "password" field filled with "passWORD$123"
  And the "hint" field filled with "account hint"
  And the "email" field filled with "test@email.com"
  But a user with "username" username and "different@email.com" email already exists
  When I click on the "sign up" button
  Then I see "The username is unavailable." error message for the "username" field

@ignore
Scenario: Field Character Limit
  Given the sign up page
  And the "username" field filled with 251 characters
  And the "password" field filled with 251 characters
  And the "hint" field filled with 251 characters
  When I click on the "sign up" button
  Then I see "The username can be at most 250 characters." error message for the "username" field
  And I see "The password can be at most 250 characters." error message for the "password" field
  And I see "The hint can be at most 250 characters." error message for the "hint" field

# Scenario: Input Below Minimum Character Limit
# Scenario: Invalid Characters in Input Fields
# Scenario: Email Address Is Case-Insensitive
# Scenario: Registration Fails Due to Server Error

@ignore
Scenario: Password Strength
  Given the sign up page
  And the "username" field filled with "username"
  And the "password" field filled with "pass"
  And the "hint" field filled with "account hint"
  When I click on the "sign up" button
  And I see "The password must be strong, at least 8 characters long containing both lowercase and uppercase letters alongside at least one numeric and special character." error message for the "password" field

@ignore
Scenario: Cancel Button
  Given the sign up page
  When I click on the "cancel" button
  Then I see the "HintKeep - Login" page

@ignore
Scenario: Cancelling Creating an Account
  Given the sign up page
  And the "username" field filled with "username"
  Then I see the "Login" page