# Context Chain

## Purpose

Manages ordered runtime contexts with explicit stage lifecycle handling.
Typical chain: user -> tenant -> micro app -> form.

Core types:

- `IContextChain`
- `IContextStage`
- `ContextStageBase`
- `IUserContext`, `UserContext`, `UserContextStage`
- `AddContextChain`, `AddContextStage<T>`, `AddUserContextStage`

## When to Use

Use when context levels depend on each other and switching one level must reliably re-evaluate dependent levels.

## How to Use

1. Register the chain and stages in DI.
2. Define clear responsibilities per stage.
3. Trigger context changes through chain APIs (not custom event meshes).

## Example Registration

```csharp
builder.Services
    .AddContextChain()
    .AddUserContextStage(order: 100);
```

## Best Practices

- Keep stage ordering deterministic.
- Ensure stage transitions are side-effect aware and reversible.
- Do not embed feature/business logic into stage orchestration.
