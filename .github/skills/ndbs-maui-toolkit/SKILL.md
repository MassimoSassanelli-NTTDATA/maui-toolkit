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
  IKeyValueStore, IAppDataRootProvider. DO NOT USE FOR: application-specific domain
  logic or HTTP/REST/OData clients.
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

- .NET 10 MAUI (`net10.0-android|ios|windows`), `Nullable` + `ImplicitUsings` enabled.
- `CommunityToolkit.Mvvm` 8.4 (`ObservableObject`, `[ObservableProperty]`, `AsyncRelayCommand`, `WeakReferenceMessenger`).
- `CommunityToolkit.Maui` 14.2.
- Root namespace `Ndbs.MauiToolkit`, one namespace per feature area.

## Setup

1. Reference the project (checked out as a sibling directory):
   `src/Ndbs.MauiToolkit/Ndbs.MauiToolkit.csproj`.
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
   the context chain (`AddContextChain()` + stages), and the environment services.

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

## Boundaries

- This is UI and application infrastructure, not application-specific domain logic.
- Keep the toolkit reusable across different .NET MAUI applications.
- Do not add dependencies from `maui-toolkit` to consuming applications.
