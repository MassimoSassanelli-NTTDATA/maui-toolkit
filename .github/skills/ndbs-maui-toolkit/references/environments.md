# System environments (`Ndbs.MauiToolkit.Environments`)

Manages one or more **system environments** — each re-targets the whole working
context (identity provider, API addresses, tenant, environment-scoped caches), not
just a single value. The model is deliberately **schema-less**: the core never knows
the concrete section types; pluggable components interpret their own sections.

## Model

- `SystemEnvironment` — a unique `Name` (identity, case-insensitive) plus an open set
  of named configuration `Sections` (`IReadOnlyDictionary<string, JsonElement>`).
  - `bool TryGetSection(string key, out JsonElement)`, `bool HasSection(string key)`.
  - Section keys are app-defined (for example `IAS`, `Xia`, additional APIs).
- `ISystemEnvironmentComponent` — a pluggable building block that interprets **one**
  section. Apps register exactly the components that describe how their environments
  are composed. The generic infrastructure delegates validation and activation to
  these components.
  - `SectionKey` (empty string ⇒ tear-down-only component: never applied, always reset).
  - `Category` — optional group for mutual-exclusion / cardinality rules
    (e.g. `IdentityProvider`, so exactly one of IAS or Entra ID may be present).
  - `IsRequired`, `Order` (apply in order, reset in reverse order).
  - `EnvironmentComponentValidation Validate(JsonElement section)`.
  - `Task ApplyAsync(section, ct)` — only when the target contains `SectionKey`.
  - `Task ResetAsync(EnvironmentSwitchOptions options, ct)` — always called during a
    switch, regardless of the target; `options.ResetActiveSession` tells the component
    whether to tear down session-scoped state (for example sign out) or keep it.

## Services

- `ISystemEnvironmentStore` / `JsonFileSystemEnvironmentStore` — persists the
  environment list and the **per-user** active selection.
  - `GetAllAsync`, `FindByNameAsync(name)`, `AddOrUpdateAsync`, `RemoveAsync(name)`.
  - `GetActiveAsync(userId)`, `SetActiveAsync(userId, name)`, `ClearActiveAsync(userId)`.
  - `userId` is `null` before login (device scope). A stored selection that no longer
    exists is treated as absent.
- `ISystemEnvironmentProvisioningService` — parses/validates configuration payloads
  (for example a scanned QR-code JSON array of environments).
  - `EnvironmentProvisioningResult Parse(payload)` — validate without storing.
  - `Task<EnvironmentProvisioningResult> ImportAsync(payload, ct)` — validate + merge
    into the store (add new, update existing by name). Invalid data is rejected with a
    business-level message.
- `ISystemEnvironmentStartupResolver` — decides the app start flow from environment
  status: **capture** when none exists, **selection** when some exist but none is
  active, otherwise **continue**.
  - `Task<EnvironmentStartupResult> ResolveAsync(userId, ct)`.
- `ISystemEnvironmentSwitchCoordinator` — the **only** allowed way to change the active
  environment. Never apply configuration directly.
  - `Task<EnvironmentSwitchResult> SwitchAsync(target, ct)` — deterministic order:
    validate → publish `EnvironmentChangingMessage` → reset all components (reverse
    order) → apply present sections (order) → persist active selection → publish
    `EnvironmentChangedMessage`.
  - `Task<EnvironmentSwitchResult> SwitchAsync(target, EnvironmentSwitchOptions, ct)` —
    same flow, but the caller decides per switch whether session-scoped state is reset
    (`EnvironmentSwitchOptions.ResetActiveSession`, default `true`). The core stays
    auth-agnostic; identity-provider components interpret the intent.
  - `Task ApplyActiveAsync(environment, ct)` — startup re-apply of an already-active
    environment **without** tear-down (no logout, no cache clearing) and without
    changing the persisted selection.

## Result types

`EnvironmentProvisioningResult`, `EnvironmentStartupResult`, `EnvironmentStartupDecision`,
`EnvironmentSwitchResult`, `EnvironmentValidationResult`, `EnvironmentComponentValidation` —
business-level outcomes; inspect them instead of throwing for expected validation
failures.

## Messaging (`Ndbs.MauiToolkit.Messaging`)

- `EnvironmentChangingMessage(Previous, Target)` — before a switch (guard unsaved work).
- `EnvironmentChangedMessage(Current)` — after the new environment has been applied.

Both are `WeakReferenceMessenger` records; subscribe with `CommunityToolkit.Mvvm`.

## Guidance

- Implement one `ISystemEnvironmentComponent` per concern (identity provider, each API)
  and register them; do not hard-code environment shape in the core.
- Always switch through `ISystemEnvironmentSwitchCoordinator`; on app start prefer
  `ApplyActiveAsync` to re-establish config without tearing anything down.
- Scope the active selection by `userId` (device scope before login).
