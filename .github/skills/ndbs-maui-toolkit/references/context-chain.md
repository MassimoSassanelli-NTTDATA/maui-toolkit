# Context chain (`Ndbs.MauiToolkit.Context`)

A generic, app-agnostic orchestration for an **ordered set of runtime contexts**
that depend on each other (for example `User → Tenant → MicroApp → Form`). The chain
knows nothing about the concrete concepts; the app contributes the stages and their
order.

## Core types

- `IContextStage` — one stage owns exactly one runtime context and knows how to build
  it up and tear it down.
  - `int Order` — position in the chain. A stage may only depend on stages with a
    **strictly lower** order. Lower = closer to the root (e.g. the user).
  - `string Name`, `bool IsActive`.
  - `Task<ContextActivation> ActivateAsync(ct)` — returns `Activated` or `Deferred`.
  - `Task DeactivateAsync(ct)` — must be idempotent.
- `ContextActivation` — `Activated` (chain continues downstream) or `Deferred`
  (a prerequisite/selection is missing; the chain stops cascading forward — this is
  **not** an error).
- `ContextStageBase` — convenience base. Tracks `IsActive`, only runs tear-down when
  active. Override `OnActivateAsync` (required) and `OnDeactivateAsync` (optional).
- `IContextChain` — orchestrates the stages (sorted by `Order`):
  - `IReadOnlyList<IContextStage> Stages`
  - `void AddStage(IContextStage)` — mainly for tests/advanced composition.
  - `Task<ContextChainResult> ActivateFromAsync(int fromOrder, ct)` — deactivates every
    stage with `Order >= fromOrder` in **reverse** order, then reactivates them in
    **forward** order until one defers.
  - `Task DeactivateFromAsync(int fromOrder, ct)` — deactivates `Order >= fromOrder` in
    reverse order.
- `ContextChainResult` — `Completed`, `DeferredAtOrder`, `DeferredStage`. Use the
  deferred info to drive the UI (e.g. show the tenant picker when the tenant stage
  deferred).
- `IUserContext` / `UserContext` — the root truth for the signed-in user
  (`UserId`, `HasUser`, `SetUser(userId)`, `Clear()`).
- `UserContextStage` — ready-made stage that activates while a user is present.

## Lifecycle patterns

- **Sign-in / initial start:** `ActivateFromAsync(firstOrder)`.
- **Sign-out:** `DeactivateFromAsync(firstOrder)`.
- **Switch at a level** (tenant, micro app, …): set the new selection, then
  `ActivateFromAsync(levelOrder)`. Every dependent stage below the level is torn down
  and rebuilt automatically, keeping the dependency direction intact.

## DI registration (`ContextChainServiceCollectionExtensions`)

```csharp
services.AddContextChain();                 // IContextChain singleton, composed from all IContextStage
services.AddUserContextStage(order: 0);     // ready-made user stage at a chosen order
services.AddContextStage<TenantContextStage>();          // simple stage
services.AddContextStage(sp => new FormContextStage(...)); // factory when Order/values are composition-time concerns
```

- `AddContextStage<TStage>()` is idempotent (safe to call from repeated setup code).
- The factory overload is the right choice when the stage's `Order` is an application
  concern decided at composition time.

## Guidance for a new stage

1. Choose an `Order` strictly greater than every stage it depends on.
2. Derive from `ContextStageBase`; put build-up in `OnActivateAsync` and return
   `ContextActivation.Deferred` when a required upstream selection is still missing.
3. Put tear-down (dispose caches, clear scoped state) in `OnDeactivateAsync`.
4. Register the stage and rely on `ActivateFromAsync` / `DeactivateFromAsync` — do not
   reimplement cascade logic in the app.
