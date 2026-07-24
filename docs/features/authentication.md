# Authentication (OIDC)

## Purpose

Provides browser-based OpenID Connect sign-in for .NET MAUI apps using the
Authorization Code Flow with PKCE, plus token storage, silent refresh, profile
mapping and sign-out. Lives in the optional, platform-targeting
`Ndbs.MauiToolkit.Auth` assembly.

Core types:

- `IAuthenticationService` / `AuthenticationService` — the primary API
- `OidcOptions`, `IOidcOptionsProvider` / `OidcOptionsProvider`, `OidcClaimMappings`
- `ITokenStore` / `SecureStorageTokenStore`, `OidcTokenSet`
- `UserProfile`, `AuthenticationResult`, `AuthenticationErrorCode`
- `BearerTokenHandler`
- `AddNdbsAuth(...)` and `AddNdbsAuthBearerToken()`
- Provider routing (base): `IActiveIdentityProvider` / `ActiveIdentityProvider`,
  `RoutingOidcClient`, `IdentityProviderOidcClient`
- IAS provider (`Ndbs.MauiToolkit.Auth.Ias`): `AddIas()`, `DuendeOidcClient`,
  `IasIdpComponent`, `IasConfig`
- Entra provider (`Ndbs.MauiToolkit.Auth.Entra`): `AddEntra()`, `MsalEntraClient`,
  `IMsalPublicClient`, `EntraIdpComponent`, `EntraConfig`

## When to Use

Use when a MAUI app authenticates users against an OIDC identity provider (for
example SAP IAS) and needs a valid access token to call protected backend APIs,
with tokens surviving app restarts and being refreshed silently.

Do not use it as an HTTP/REST/OData client — it only obtains and attaches tokens.
Model the actual API calls with your HTTP client of choice (the toolkit stays out
of that concern).

## Assembly and Dependencies

- The base assembly `Ndbs.MauiToolkit.Auth` (`net10.0-android|ios|windows`,
  `UseMaui`) is **provider-agnostic**: it owns the orchestration
  (`IAuthenticationService`), the token store, the `IOidcClient` seam and the
  provider routing (`RoutingOidcClient`, `IActiveIdentityProvider`). It no longer
  depends on any concrete identity-provider SDK.
- References `Ndbs.MauiToolkit` (and transitively `Ndbs.MauiToolkit.Core`) — it
  integrates with the toolkit's environment services and messaging. Pulls in
  `CommunityToolkit.Mvvm` (state notifications) and `Microsoft.Extensions.Http`.
- Each identity provider is a separate, optional add-on project that references the
  base and registers itself through the routing seam:
  - `Ndbs.MauiToolkit.Auth.Ias` — SAP IAS via `Duende.IdentityModel.OidcClient`
    (Authorization Code Flow with PKCE) and the MAUI system browser; enabled with
    `AddIas()`.
  - `Ndbs.MauiToolkit.Auth.Entra` — Azure Entra ID via `Microsoft.Identity.Client`
    (MSAL.NET); enabled with `AddEntra()`.
- Reference the base plus exactly the provider projects an app needs, so an
  IAS-only app never pulls in another provider's SDK.

## How to Use

1. Register the services and configure the identity provider. The base registration
   is provider-agnostic; add the identity provider(s) the app uses (for example IAS)
   through the returned builder. The configuration is validated eagerly, so missing
   mandatory parameters fail fast:

   ```csharp
   builder.Services.AddNdbsAuth(options =>
   {
       options.Authority = "https://your-tenant.accounts.ondemand.com";
       options.ClientId = "mobile-app";
       options.RedirectUri = "myapp://callback";
       // options.Scopes, options.EnableRpInitiatedLogout, options.Browser, ...
   })
   .AddIas(); // enable the SAP IAS provider (Ndbs.MauiToolkit.Auth.Ias)
   ```

2. Optionally attach the access token to an `HttpClient` automatically:

   ```csharp
   builder.Services
       .AddHttpClient("backend")
       .AddNdbsAuthBearerToken();
   ```

3. Drive the lifecycle from a view model through `IAuthenticationService`:

   ```csharp
   AuthenticationResult result = await auth.LoginAsync();

   string? token = await auth.GetAccessTokenAsync();      // silent refresh when expired
   bool signedIn = await auth.IsAuthenticatedAsync();
   UserProfile? profile = await auth.GetUserProfileAsync();

   await auth.LogoutAsync(remoteLogout: true);            // clears tokens; optional RP logout
   ```

4. React to sign-in/sign-out via `WeakReferenceMessenger` using
   `AuthenticationStateChangedMessage`.

## Runtime Configuration and Environment Switching

- `IOidcOptionsProvider` exposes an immutable snapshot of `OidcOptions` and allows the
  `Authority` / `ClientId` to be updated at runtime, so the app can point at a
  different identity provider without a restart.
- `IasIdpComponent` (an `ISystemEnvironmentComponent` in `Ndbs.MauiToolkit.Auth.Ias`)
  plugs the IAS provider into the toolkit's environment services: on apply it
  reconfigures the authority and client id from the environment definition and
  activates the IAS provider through `IActiveIdentityProvider`; on tear-down it signs
  the user out, so a session never survives an environment switch.
- `EntraIdpComponent` (an `ISystemEnvironmentComponent` in
  `Ndbs.MauiToolkit.Auth.Entra`) does the same for Azure Entra ID: on apply it sets
  the tenant authority and client id and activates the Entra/MSAL provider; on
  tear-down it signs the user out. Because both components share the
  `IdentityProvider` category, an environment must configure exactly one of IAS or
  Entra — never both.

## Security

- PKCE is always used and no client secret is required.
- Tokens are persisted through `ISecureStorage` (`MauiSecureStorage` over the
  platform secure storage), not in plain preferences.
- Concurrent `GetAccessTokenAsync` callers share a single refresh (the service is a
  singleton so the refresh lock is app-wide).
- On Windows the embedded web view is used for sign-in because the system-browser
  `WebAuthenticator` is unavailable there.

## Best Practices

- Call operations from view models against `IAuthenticationService`; do not use the
  Duende client or platform browser directly.
- Inspect `AuthenticationResult.ErrorCode` (`AuthenticationErrorCode`) to react to
  offline, cancelled, protocol and token-validation cases instead of parsing messages.
- Register a real `INetworkConnectivity` implementation from the app; the library
  falls back to "always online" only as a default.
- Keep this capability optional: reference the assembly only from apps that sign in.
