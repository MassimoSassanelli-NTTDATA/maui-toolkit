---
name: ndbs-maui-toolkit
description: >
  Reusable .NET MAUI building blocks in the Ndbs.MauiToolkit library (namespace
  Ndbs.MauiToolkit). USE BEFORE creating app-local or solution-local view-model bases, Shell
  navigation, dialogs, icon helpers, context/tenant chains or system-environment
  switching in .NET MAUI applications. USE FOR: BaseViewModel / PageBase / ShellBase,
  INavigationService, IDialogService, UseMauiNdbsToolkit DI registration,
  IContextChain (User -> Tenant -> MicroApp -> Form), SystemEnvironment
  provisioning + switching, PlatformIconView / AppIconCatalog, ListDiffAnalyzer,
  IKeyValueStore, IAppDataRootProvider, the optional DynamicTables assembly
  (runtime SQLite tables + CSV import: IDynamicTableRepository,
  IDynamicTableImportService, IDynamicTableMemoryCache, AddDynamicTables), and the
  optional Auth assembly (browser-based OIDC sign-in with PKCE, secure token storage,
  silent refresh, HttpClient bearer tokens: IAuthenticationService, OidcOptions,
  AddNdbsAuth, AddNdbsAuthBearerToken, BearerTokenHandler). DO NOT USE
  FOR: application-specific domain logic or HTTP/REST/OData clients.
---

# Ndbs.MauiToolkit

`Ndbs.MauiToolkit` is a reusable **.NET MAUI component library**.
It provides reusable MVVM bases,
navigation/dialog services, a generic runtime-context chain, system-environment
management, cross-platform icons and small utilities.

**Golden rule — reuse before create:** Before adding an app-local view-model base,
navigation service, dialog helper, icon control, tenant/context orchestration or
environment switching in a .NET MAUI app, check whether this toolkit already provides it
and use it. Only introduce a new local component when nothing here fits, and prefer
extending the toolkit (as a follow-up task in the `maui-toolkit` repository) over
duplicating a capability.

**Workflow rule for consumers of this toolkit:** If a request is about view models,
pages, Shell navigation, dialogs, icons, context chains, or environment switching,
read this skill first. Then verify the guidance against the concrete toolkit code by
reading only the most relevant implementation files, typically 2 to 4 files such as
`BaseViewModel`, `PageBase<TViewModel>`, `ShellBase<TViewModel>`, service
interfaces, or DI registration. Use a direct code-first entry only when the request
targets a specific local bug, file, symbol, or runtime behavior.

## Tech baseline

- Two core assemblies plus two optional add-on assemblies:
  - `Ndbs.MauiToolkit.Core` (`net10.0`, no MAUI dependency) — platform-neutral logic
    (diff utilities, environments, context chain, messaging, and the `IKeyValueStore`
    / `IAppDataRootProvider` abstractions). Project-referenceable from platform-neutral
    unit test projects.
  - `Ndbs.MauiToolkit` (`net10.0-android|ios|windows`) — references Core and adds the
    MAUI implementations (MVVM bases, Shell navigation, dialogs, icons, `Preferences`
    / `FileSystem` defaults, and DI wiring).
  - `Ndbs.MauiToolkit.DynamicTables` (`net10.0`, no MAUI dependency) — **optional**
    add-on for runtime SQLite table creation and CSV import over an existing EF Core
    `DbContext`. Depends on `Microsoft.EntityFrameworkCore.Sqlite` and `CsvHelper`;
    reference it only from apps that need dynamic tabular import. It is independent
    of Core and the MAUI assembly (no project reference between them).
  - `Ndbs.MauiToolkit.Auth` (`net10.0-android|ios|windows`, `UseMaui`) — **optional**
    **provider-agnostic** base for OIDC sign-in: orchestration
    (`IAuthenticationService`), secure token storage, silent refresh, profile mapping,
    `HttpClient` bearer-token wiring and the provider-routing seam
    (`RoutingOidcClient` / `IActiveIdentityProvider`). References `Ndbs.MauiToolkit`
    (and transitively Core); it no longer depends on a concrete identity-provider SDK.
    Reference it only from apps that authenticate, together with one or more provider
    add-ons.
  - `Ndbs.MauiToolkit.Auth.Ias` (`net10.0-android|ios|windows`, `UseMaui`) —
    **optional** IAS identity-provider add-on for `Ndbs.MauiToolkit.Auth`. Provides
    the `Duende.IdentityModel.OidcClient`-based client, the MAUI system browser and
    `IasIdpComponent`; enabled with `AddIas()`. References `Ndbs.MauiToolkit.Auth` and
    pulls in `Duende.IdentityModel.OidcClient`. Reference it only from apps that use
    SAP IAS.
- `Nullable` + `ImplicitUsings` enabled.
- `CommunityToolkit.Mvvm` 8.4 (`ObservableObject`, `[ObservableProperty]`, `AsyncRelayCommand`, `WeakReferenceMessenger`).
- `CommunityToolkit.Maui` 14.2.
- Root namespace `Ndbs.MauiToolkit`, one namespace per feature area. Namespaces are
  shared across the assemblies (an area lives in exactly one of them).

## Setup

1. Reference the projects (checked out as a sibling directory):
   - App / MAUI consumers: `src/Ndbs.MauiToolkit/Ndbs.MauiToolkit.csproj`
     (transitively pulls in `Ndbs.MauiToolkit.Core`).
   - Platform-neutral test/library consumers that only need the core logic:
     `src/Ndbs.MauiToolkit.Core/Ndbs.MauiToolkit.Core.csproj`.
   - Optional, only when importing dynamic tabular data:
     `src/Ndbs.MauiToolkit.DynamicTables/Ndbs.MauiToolkit.DynamicTables.csproj`.
2. Register the default services in `MauiProgram`:

   ```csharp
   builder.Services.UseMauiNdbsToolkit();
   ```

   This registers `INavigationService` → `NavigationService`,
   `IDialogService` → `DialogService`, every `AppIconCatalog` icon, and
   `IPlatformIconResolver` → `PlatformIconResolver`.
3. To use `PlatformIconView`, also register its handler:

   ```csharp
   builder.ConfigureMauiHandlers(h => h.AddNdbsPlatformIconHandlers());
   ```

4. **Not** auto-registered — register in the app when you need them:
   `IKeyValueStore` → `PreferencesKeyValueStore`,
   `IAppDataRootProvider` → `MauiAppDataRootProvider`,
   the context chain (`AddContextChain()` + stages), the environment services,
   the optional dynamic tables (`AddDynamicTables<TContext>()`), and the optional
   authentication library (`AddNdbsAuth(...)` plus one provider add-on such as
   `AddIas()`, and `AddNdbsAuthBearerToken()` on an `IHttpClientBuilder`).

## Capability map

| Area (namespace) | Use it for | Key types |
|---|---|---|
| **MVVM** (`Ndbs.MauiToolkit.Mvvm`) | View-model base with busy-state + async init; page/shell bases that auto-run `InitializeAsync` on appear | `BaseViewModel` (`IsBusy`, `IsBusyFor`, `InitializeAsyncCommand`, `IQueryAttributable`), `IInitializableViewModel`, `PageBase` / `PageBase<TViewModel>`, `ShellBase` / `ShellBase<TViewModel>` |
| **Navigation & dialogs** (`Ndbs.MauiToolkit.Services`) | Shell navigation and modal alerts/confirms from view models without UI dependencies | `INavigationService.NavigateToAsync(route, parameters?)` / `GoBackAsync(parameters?)`, `IDialogService.ConfirmAsync(...)` / `AlertAsync(...)` |
| **DI / Startup** (`Ndbs.MauiToolkit`, `.Startup`) | One-call service registration; testable preferences store | `UseMauiNdbsToolkit()`, `IKeyValueStore` / `PreferencesKeyValueStore` |
| **Context chain** (`Ndbs.MauiToolkit.Context`) | Ordered runtime-context dependency chain (User → Tenant → MicroApp → Form) with activate / deactivate / defer on sign-in, sign-out and level switches | `IContextChain`, `IContextStage`, `ContextStageBase`, `IUserContext` / `UserContext`, `UserContextStage`, `AddContextChain()` / `AddContextStage<T>()` / `AddUserContextStage(order)` — see `references/context-chain.md` |
| **System environments** (`Ndbs.MauiToolkit.Environments`) | Schema-less environment model, QR/JSON provisioning, startup decision, controlled environment switch with tear-down/apply | `SystemEnvironment`, `ISystemEnvironmentStore` / `JsonFileSystemEnvironmentStore`, `ISystemEnvironmentProvisioningService`, `ISystemEnvironmentStartupResolver`, `ISystemEnvironmentSwitchCoordinator`, `ISystemEnvironmentComponent` — see `references/environments.md` |
| **Messaging** (`Ndbs.MauiToolkit.Messaging`) | React to environment switches via `WeakReferenceMessenger` | `EnvironmentChangingMessage(Previous, Target)`, `EnvironmentChangedMessage(Current)` |
| **Icons** (`Ndbs.MauiToolkit.Icons`) | Platform-native icons (SF Symbols on iOS, Material Symbols on Android/Windows) | `PlatformIconView` (XAML: `IconKey`, `Size`, `Color`), `AppIconCatalog`, `IPlatformIconResolver`, `AddNdbsPlatformIconHandlers()` |
| **Diff** (`Ndbs.MauiToolkit.Diff`) | Compare a remote and a local list into added / removed / changed / unchanged | `ListDiffAnalyzer<T>(idSelector, isChangedFunc).Calculate(remote, local)` → `DiffDescription<T>` (`DiffTypes`) |
| **Workspace** (`Ndbs.MauiToolkit.Workspace`) | Abstracted app-data root path (testable over `FileSystem.AppDataDirectory`) | `IAppDataRootProvider` / `MauiAppDataRootProvider` |
| **Dynamic tables** (`Ndbs.MauiToolkit.DynamicTables`, optional assembly) | Runtime SQLite table creation + CSV import over an existing EF Core `DbContext`; identifier-validated and parameterized against SQL injection | `AddDynamicTables<TContext>()`, `IDynamicTableRepository`, `IDynamicTableImportService`, `IDynamicTableMemoryCache`, `CsvParser`, `CsvImportOptions`, `CsvImportResult` — see `../../docs/features/dynamic-tables.md` |
| **Authentication** (`Ndbs.MauiToolkit.Auth` + provider add-ons) | Provider-agnostic OIDC sign-in: orchestration, secure token storage, silent refresh, profile mapping, sign-out, `HttpClient` bearer tokens and per-environment provider routing. Concrete providers are separate optional projects (SAP IAS in `Ndbs.MauiToolkit.Auth.Ias` via `AddIas()`); exactly one identity provider is active per environment | base: `AddNdbsAuth(...)`, `AddNdbsAuthBearerToken()`, `IAuthenticationService` (`LoginAsync` / `GetAccessTokenAsync` / `LogoutAsync` / `IsAuthenticatedAsync` / `GetUserProfileAsync`), `OidcOptions`, `IOidcOptionsProvider`, `ITokenStore`, `IActiveIdentityProvider`, `RoutingOidcClient`, `IdentityProviderOidcClient`, `UserProfile`, `AuthenticationResult` / `AuthenticationErrorCode`, `BearerTokenHandler`, `AuthenticationStateChangedMessage`; IAS: `AddIas()`, `IasIdpComponent` — see `../../docs/features/authentication.md` |

## When to reach for what

- **New page or view model?** Derive the view model from `BaseViewModel`, the page
  from `PageBase<TViewModel>` (or the shell root from `ShellBase<TViewModel>`), and
  put load logic in `InitializeAsync` — it runs automatically on appear. Wrap
  long-running work in `IsBusyFor(...)` to drive `IsBusy`.
- **Navigate or show a dialog?** Inject `INavigationService` / `IDialogService`;
  never call `Shell.Current` or `DisplayAlert` directly from a view model.
- **Icons in XAML?** Use `PlatformIconView` with an `IconKey` from `AppIconCatalog`
  and `AppThemeBinding` for the `Color`.
- **Tenant / micro-app / form context that cascades?** Use the context chain instead
  of ad-hoc reload logic — read `references/context-chain.md` first.
- **Multiple backend/identity environments, QR onboarding, environment switch?**
  Use the environment services — read `references/environments.md` first.
- **Sync reconciliation (local vs. remote list)?** Use `ListDiffAnalyzer<T>`.
- **Import tabular data with an unknown-at-compile-time schema (e.g. user CSV)?**
  Reference the optional `Ndbs.MauiToolkit.DynamicTables` assembly and use
  `AddDynamicTables<TContext>()` with `IDynamicTableImportService`. Do not use it for
  fixed, known domain schemas — model those as normal EF Core entities.
- **Sign users in against an OIDC identity provider (e.g. SAP IAS) and call protected
  APIs?** Reference the provider-agnostic `Ndbs.MauiToolkit.Auth` base plus the
  provider add-on the app needs (SAP IAS: `Ndbs.MauiToolkit.Auth.Ias`), register with
  `AddNdbsAuth(...).AddIas()`, drive the lifecycle through `IAuthenticationService`,
  and attach tokens with `AddNdbsAuthBearerToken()`. Do not call the OIDC client or
  platform browser directly, and do not use it as a REST/OData client — read
  `../../docs/features/authentication.md` first.

## Boundaries

- This is UI and application infrastructure, not application-specific domain logic.
- Keep the toolkit reusable across different .NET MAUI applications.
- Do not add dependencies from `maui-toolkit` to consuming applications.
