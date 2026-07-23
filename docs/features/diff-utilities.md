# Diff Utilities

## Purpose

Compares remote and local collections and classifies entries for synchronization and reconciliation.

Core types:

- `ListDiffAnalyzer<T>`
- `DiffDescription<T>`
- `DiffTypes`

## When to Use

Use when local/offline data and server data must be merged or reconciled deterministically.

## How to Use

1. Create analyzer with ID selector and change predicate.
2. Call `Calculate(remote, local)`.
3. Process added, removed, changed, and unchanged sets.

## Example

```csharp
var analyzer = new ListDiffAnalyzer<Item>(
    idSelector: x => x.Id,
    isChangedFunc: (remote, local) => remote.Version != local.Version);

var diff = analyzer.Calculate(remoteItems, localItems);
```

## Best Practices

- Use stable IDs.
- Keep change predicate explicit and domain-safe.
- Treat unchanged sets as first-class output for diagnostics.
