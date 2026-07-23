# Workspace Abstraction

## Purpose

Abstracts app-data root access to improve testability and decouple code from static filesystem APIs.

Core types:

- `IAppDataRootProvider`
- `MauiAppDataRootProvider`
- `IKeyValueStore`
- `PreferencesKeyValueStore`

## When to Use

Use in services that need app-local storage paths or lightweight key/value persistence.

## How to Use

1. Inject `IAppDataRootProvider` where file paths are needed.
2. Inject `IKeyValueStore` for simple preferences/state data.
3. Replace implementations in tests with in-memory or temp-folder variants.

## Example

```csharp
public sealed class CacheService
{
    private readonly IAppDataRootProvider _appDataRoot;

    public CacheService(IAppDataRootProvider appDataRoot)
    {
        _appDataRoot = appDataRoot;
    }
}
```

## Best Practices

- Keep storage paths centralized behind abstractions.
- Avoid direct `FileSystem.AppDataDirectory` access in feature code.
- Use mockable implementations for unit and integration tests.
