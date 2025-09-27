# `AuthenticateRequestHandler`

## Request: `AuthenticateRequest`

| Property  | Type   | Validation Attributes | Description                |
|-----------|--------|----------------------|----------------------------|
| Username  | string | `[Required]`         | A username is required.    |
| Password  | string | `[Required]`         | A password is required.    |

## Result: `AuthenticateResult`

| Property                | Type     | Description                        |
|-------------------------|----------|------------------------------------|
| UserId                  | Guid     | The authenticated user's ID.       |
| SessionId               | Guid     | The session ID.                    |
| Username                | string   | The authenticated username.        |
| SessionToken            | string   | The session token.                 |
| SessionTokenExpiration  | DateTime | When the session token expires.    |
| SessionTicket           | string   | The session ticket.                |
| SessionTicketExpiration | DateTime | When the session ticket expires.   |

## Dependencies

- **Depends on:**
  - [`CreateSessionTokenRequestHandler`](./path/to/CreateSessionTokenRequestHandler.md)
  - [`CreateSessionTicketRequestHandler`](./path/to/CreateSessionTicketRequestHandler.md)

- **Depended on by:**
  - _[List other request handlers that inject or use this handler, with links if possible]_

## Source

[View source on GitHub](../GraphQL/Definitions/Users/Accounts/AuthenticateRequestHandler.cs)
