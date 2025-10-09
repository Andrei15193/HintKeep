# HintKeep Stories: Sweet Authentication

This feels like a topic that is ever present for any application, authentication followed up by authorization.

Authentication is a constant presence in any application. While the industry has largely converged on JSON Web Tokens and multi-factor authentication as the standard, implementation paths in general boil down to two solutions: build your own or use an external Identity Provider such as Entra ID or sign-in with GitHub or any other platform that supports this.

Already existing identity providers resolve many challenges as they offer an out of the box solution for managing users, handle register and authentication flows, however they can be limiting and require navigation between applications which may be tracked by the vendor.

For HintKeep I have decided to build something custom for the following reasons:
* Support the base idea of the application, provide a recovery method that emails you the account hint
* Learn more about current standards and options for handling authentication tokens
* Improve privacy by not relying on a 3rd party for signing up and logging into the application.

## Acquiring and Sending JWTs

In HintKeep, a JSON Web Token (JWT) is acquired either when a user registers or authenticates. The token contains just enough information to verify authenticity and identify the user and their current session, all done through claims plus an expiration. JWTs are typically short-lived, requiring a refresh while the user remains active.

Once issued by the web API, the token is passed with future requests through the Authorization header using the Bearer scheme. This is a widely used standard, and one of the preferred methods for handling authentication.

On the server side, a middleware intercepts incoming requests, validates the token, and determines whether the user is authenticated. If validation fails, the user is either denied access or treated as anonymous.

One subtle design choice is to avoid revealing why a token is invalid. This is a form of pseudo-security, **security through ambiguity**. It slows down attackers by obscuring the authentication logic, but it does not make the system truly secure. Without robust policies and safeguards, ambiguity is just a delay tactic. Given enough time and interest, any system can be breached.

## Maintaining Login

The next challenge in this undertaking is keeping the user logged in across page refreshes or browser restarts. If they return within a reasonable timeframe, such as one or two days, it is ideal to preserve their session. Re-entering credentials adds friction and makes the application less appealing to use.

The simplest solution is to store the JSON Web Token (JWT) in local or session storage. When the user returns, the token is retrieved and passed to the web API as if they never left. If the token has expired, they’re redirected to log in again.

But this approach has two major problems.

### Problem 1: Short-Lived Tokens

JWTs in HintKeep are intentionally short-lived, around 30 minutes, to reduce attack window. If an attacker intercepts a request (e.g., through a man-in-the-middle attack), they must act quickly to exploit the token. This short lifespan is sufficient for page refreshes, but not for longer absences.

To address this, HintKeep issues an additional "session starter" token when the user registers or authenticates. This token is long-lived, between 2 and 3 days. It can only initiate a new session, it does not work for any other operation. The token is only provided to the web API when the application loads.

This reduces the attack surface as the token is rarely exposed through backend calls, unlike the authenticaiton token.

### Problem 2: Storage Vulnerabilities

Storing JWTs or any authentication data in local or session storage exposes the application to **cross-site scripting** (XSS) attacks. A malicious actor could use an iframe, redirect, or injected script to extract sensitive data from browser storage without the user realizing it.

For internal tools, this method may be acceptable due to other layers of protection, such as VPN access or network-level restrictions. However, HintKeep is a public facing application increasing the risk.

## Revisit Sending JWTs

The core issue with maintaining login is the exposure of access tokens in JavaScript-accessible storage.

As an alternative, tokens can be passed to and from the web API using HTTP-only cookies. These are managed automatically by the browser and never exposed to JavaScript. The cookie’s expiration, domain and path are set by the server, ensuring controlled scope and lifecycle.

One trade-off is that the browser does not distinguish between tabs. Opening the application in a new tab will automatically recognize the current session and authenticate the user. In most cases, this is desirable. However, it can be tricky during testing or when a user has multiple accounts and wants to move data between them.

Instead of using the Authorization header and managing JWTs manually in the frontend, HintKeep moves all session handling to HTTP-only cookies. Each cookie is set to expire at the same time as the token it contains, avoiding lingering data.

### Session Header

The challenge of supporting multiple sessions simultaneously remains. While this can seem like an edge case, having a mechanism in place increases flexibility and enables more intuitive interactions.

To distinguish between sessions, a unique ID is appended to the cookie name that stores the authentication token. Authenticating a user now involves two headers.

* A cookie sent automatically by the browser
* An `X-HintKeep-Session-Id` header containing the session ID associated with the token

This allows application code to target the correct session cookie, even if multiple are present from past logins or page refreshes. These cookies should clear out quickly though.

## Tickets and Tokens

While revisiting the authentication flow, I uncovered a subtle vulnerability: the session starter token. Although it permits only one operation, initiating a session, if hijacked it can still be used to impersonate a user without their knowledge. This mirrors the risk posed by authentication tokens, although in a more restricted context.

The problem lies in its lifespan. The session starter token is designed to be long-lived, between 2 and 3 days, to provide a smooth user experience. It is stored as an HTTP-only cookie, which makes it harder for attackers to access through JavaScript. But the risk remains, if stolen the token can be used to start sessions, giving an attacker ample time to exploit it.

After consulting with Copilot (chat), it came up with a solution: the session starter token should be **one-use only**, similar to a ticket. Once a session is started using this token, it becomes invalid. This limits the attack window to a single opportunity.

This refinement led to a clearer terminology split:

* **Tickets**: One-time-use tokens, the "session starter token" is now called session ticket.
* **Tokens**: Multi-use and short-lived, like the authentication token.

A session ticket is issued by the application and can start exactly one session. Once used, even if the operation fails after validation, it cannot be reused. If you board a bus and it breaks down, you need a new ticket. You might get reimbursed, but not in HintKeep.

## Session Token Renewal

The final piece of the authentication puzzle is session renewal. After registering or logging in, the authentication token lasts for 30 minutes. But users may remain active for longer. Forcing them to re-authenticate every half hour is both cumbersome and annoying.

To solve this, HintKeep issues a **renew ticket** alongside the authentication token and session ID. Similar to the session ticket, the renew ticket can be used only once.

Session renewal happens in the background. Every 10 to 20 minutes, the application calls the backend to request a new JSON Web Token for the same session. Using the renew ticket, the session token is updated in the corresponding cookie and a new renew ticket is returned in the response body.

## Complete Flow & Conclusion

Time to wrap things up.

Throughout this post, we explored several major concerns around authentication and how HintKeep addresses them.

* **Token safety**: avoiding JavaScript-accessible storage to mitigate cross-site scripting (XSS) risks.
* **Session isolation**: supporting concurrent sessions via a dedicated session ID header.
* **Single-use tokens**: introducing tickets to start a new session for returning users and to reduce replay risk.
* **Session renewal**: maintaining active sessions through one-time-use renew tickets.

To make things easier to follow, the entire flow is summarized in the diagram below.

```eraser.io
title HintKeep Authentication
styleMode plain
typeface mono

Client [icon: user]
Backend [icon: server]

Client --> Backend: register & login
Client <-- Backend: session ticket cookie
Client <-- Backend: session token cookie
Client <-- Backend: session ID & renew ticket response
```

```eraser.io
title HintKeep Returning User
styleMode plain
typeface mono

Client [icon: user]
Backend [icon: server]

Client --> Backend: begin session + ticket
Client <-- Backend: session ticket cookie
Client <-- Backend: session token cookie
Client <-- Backend: session ID & renew ticket response
```

```eraser.io
title HintKeep Session Renewal
styleMode plain
typeface mono

Client [icon: user]
Backend [icon: server]

Client --> Backend: renew session + ticket
Client <-- Backend: session token cookie
Client <-- Backend: renew ticket response
```