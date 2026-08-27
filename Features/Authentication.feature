Feature: Authentication

As a user
I want to be able to log in
So that I can use the app

@ignore-graphql
Scenario: Login page
  Given the landing page
  Then I see the "Login" page
  * I see the "username" field
  * I see the "password" field
  * I see the "login" button
  * I see the "login with LocalDB" button
  * I see the "sign up" link

@ignore-graphql
Scenario: Login with missing account
  Given the landing page
  When I enter "test" for "Username"
  And I enter "pa$$w0rd123" for "Password"
  And I click on the login button
  Then I see "Wrong credentials. Try again or follow the password recovery steps." error message for the "Username" field

@ignore-graphql
Scenario: Login with existing account and wrong credentials
  Given the landing page
  And there is an existing user with "test" username, "pa$$w0rd123" password and "test hint" hint
  When I enter "test" for "Username"
  And I enter "wrong pa$$w0rd123" for "Password"
  And I click on the login button
  Then I see "Wrong credentials. Try again or follow the password recovery steps." error message for the "Username" field

@ignore-graphql
Scenario: Login with existing account and matching credentials
  Given the landing page
  And there is an existing user with "test" username, "pa$$w0rd123" password and "test hint" hint
  When I enter "test" for "Username"
  And I enter "pa$$w0rd123" for "Password"
  And I click on the login button
  Then I see the "Accounts" page