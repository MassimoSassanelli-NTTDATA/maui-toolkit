# Xia Toolkit

The optional `Ndbs.MauiToolkit.Xia` assembly provides XIA-specific building blocks
for MAUI applications that work with tenants, micro apps, tenant-scoped storage,
and micro-app synchronization.

## What It Solves

Use this assembly when your app needs to:

- maintain tenant and micro-app runtime context
- compose a context chain (user -> tenant -> micro app)
- resolve and manage tenant-specific workspace paths
- coordinate tenant switching with pluggable guards and hooks
- register micro-app synchronization infrastructure and package handling
- compose system-environment components for XIA scenarios

## Key Types

- `UseMauiNdbsXia(...)`: main DI registration entry point
- `AddXiaContextChain`, `AddXiaTenantStage`, `AddXiaMicroAppStage`: context-chain composition
- `ITenantContext`, `IMicroAppContext`: active runtime contexts
- `ITenantWorkspaceProvider`: tenant workspace path abstraction
- `ITenantSwitchCoordinator`: tenant switch orchestration
- `ISyncService` + `AddXiaMicroAppSync(...)`: micro-app sync registration
- `UseMauiNdbsSystemEnvironments`, `AddEnvironmentComponent<T>`: environment composition

## Runtime Flow and Dependencies

The central runtime dependency flow is:

`Session/User -> Environment -> Tenant -> MicroApp`

### 1. Session/User as prerequisite

- The context chain is built around ordered stages (typically user -> tenant -> micro app).
- Tenant and micro-app activation are deferred until a user session exists (`IUserContext`).
- Re-activating from a stage order automatically tears down all dependent lower stages first.

### 2. Environment switch configures runtime, not tenant context directly

- `ISystemEnvironmentSwitchCoordinator` performs a deterministic switch sequence:
    validate -> publish changing -> reset components (reverse order) -> apply components (order) -> persist active environment -> publish changed.
- Session behavior is controlled by `EnvironmentSwitchOptions.ResetActiveSession`:
    - `true` (default): components may clear active sign-in/session state.
    - `false`: components that support it can keep the current session alive.
- The XIA environment component (`XiaComponent`) updates API base URL and stores tenant configuration for later use, but does not switch tenant context itself.

### 3. Tenant activation depends on user + selection/config

- `TenantContextStage` runs only after user context is available.
- Target tenant comes from explicit `TenantSelection`, environment tenant configuration, or current tenant fallback.
- Tenant activation goes through the tenant switch coordinator and then updates `ITenantContext` and tenant workspace state.

### 4. MicroApp activation depends on active tenant

- `MicroAppContextStage` requires both user and active tenant.
- The stage resolves the selected micro app, creates/opens tenant-scoped micro-app workspace, then updates `IMicroAppContext`.
- Without tenant or micro-app selection, activation defers intentionally.

### 5. Dependency behavior during changes

- Environment change can trigger reset/apply behavior across session-related components first.
- Tenant change rebuilds downstream context (especially micro app) by activating the chain from the tenant stage order.
- Micro-app change rebuilds only micro-app-and-below stages.

This keeps the dependency direction explicit: upstream context changes invalidate and rebuild downstream runtime context, never the other way around.

## DI Registration

```csharp
builder.Services
    .UseMauiNdbsToolkit()
    .UseMauiNdbsXia(options =>
    {
        options.BaseUri = new Uri("https://example.invalid/");
    })
    .AddXiaMicroAppSync();

builder.Services
    .UseMauiNdbsSystemEnvironments()
    .AddEnvironmentComponent<LocalDataComponent>();
```

## Notes for Tests

`Ndbs.MauiToolkit.Xia` targets MAUI platform TFMs and is not directly
project-referenceable from a plain `net10.0` unit test project. The Xia test project
therefore compiles the platform-neutral Xia source files as shared source and excludes
MAUI-specific defaults and DI/bootstrap code.

## Boundaries

- This assembly is XIA-focused application infrastructure.
- Keep domain-specific business logic outside the toolkit.
- Prefer extension points and interfaces over app-specific coupling.
