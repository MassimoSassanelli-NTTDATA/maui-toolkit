# Ndbs.MauiToolkit

Reusable building blocks for .NET MAUI applications.

Goal: reduce app-local infrastructure code and provide consistent, production-ready patterns for MVVM, navigation, context handling, environment switching, and cross-platform UI utilities.

## What It Is For

Use this toolkit when your MAUI app needs:

- shared MVVM base classes for pages and view models
- navigation and dialog abstractions for view-model driven UI
- ordered runtime context handling (for example user -> tenant -> micro app -> form)
- environment provisioning and controlled environment switching
- cross-platform icon abstraction
- list diffing utilities for sync/reconciliation scenarios
- app-data/workspace path abstractions

## Main Features

- MVVM foundation
  - `BaseViewModel`, `PageBase`, `ShellBase`
  - async initialization and busy-state helpers
  - Docs: [docs/features/mvvm-foundation.md](docs/features/mvvm-foundation.md)
- Navigation and dialogs
  - `INavigationService` for route-based navigation
  - `IDialogService` for confirm/alert usage from view models
  - Docs: [docs/features/navigation-and-dialogs.md](docs/features/navigation-and-dialogs.md)
- Context chain
  - `IContextChain` and stage model for ordered activation/deactivation
  - Docs: [docs/features/context-chain.md](docs/features/context-chain.md)
- Environment services
  - provisioning, startup resolving, and safe environment switching
  - Docs: [docs/features/environment-services.md](docs/features/environment-services.md)
- Icons
  - `PlatformIconView` and icon catalog/resolver abstraction
  - Docs: [docs/features/icons.md](docs/features/icons.md)
- Diff utilities
  - `ListDiffAnalyzer<T>` for local/remote comparison
  - Docs: [docs/features/diff-utilities.md](docs/features/diff-utilities.md)
- Workspace abstraction
  - app-data root provider abstractions for testable file operations
  - Docs: [docs/features/workspace-abstraction.md](docs/features/workspace-abstraction.md)

## Developer Documentation

- [docs/features/mvvm-foundation.md](docs/features/mvvm-foundation.md)
- [docs/features/navigation-and-dialogs.md](docs/features/navigation-and-dialogs.md)
- [docs/features/context-chain.md](docs/features/context-chain.md)
- [docs/features/environment-services.md](docs/features/environment-services.md)
- [docs/features/icons.md](docs/features/icons.md)
- [docs/features/diff-utilities.md](docs/features/diff-utilities.md)
- [docs/features/workspace-abstraction.md](docs/features/workspace-abstraction.md)

## Quick Start

1. Reference the toolkit project/package in your MAUI app.
2. Register toolkit services in `MauiProgram`:

```csharp
builder.Services.UseMauiNdbsToolkit();
```

3. If you use platform icons, register handlers:

```csharp
builder.ConfigureMauiHandlers(h => h.AddNdbsPlatformIconHandlers());
```

4. In new screens, prefer toolkit primitives first:
   - derive view models from `BaseViewModel`
   - use `PageBase<TViewModel>` or `ShellBase<TViewModel>`
   - inject `INavigationService` and `IDialogService`

## Typical Usage Guidance

- New page with load flow: put load logic into the view model initialization flow.
- Navigation from view models: use `INavigationService` instead of UI-static calls.
- User prompts from view models: use `IDialogService`.
- Multi-step runtime context dependencies: use the context chain rather than custom event wiring.
- Sync reconciliation: use `ListDiffAnalyzer<T>`.

## Scope and Boundaries

- This toolkit is for reusable app infrastructure, not app-specific business logic.
- Keep additions framework-level and cross-app reusable.
- Avoid coupling the toolkit to consuming app domain models.

## Principle

Reuse before create: check the toolkit first before introducing app-local alternatives.
