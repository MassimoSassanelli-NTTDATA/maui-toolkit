# MVVM Foundation

## Purpose

Provides reusable base types for view models and pages to standardize initialization and busy-state handling.

Core types:

- `BaseViewModel`
- `IInitializableViewModel`
- `PageBase` and `PageBase<TViewModel>`
- `ShellBase` and `ShellBase<TViewModel>`

## When to Use

Use this feature for new screens and shell roots where initialization logic should run consistently and async work should expose a busy state.

## How to Use

1. Derive your view model from `BaseViewModel`.
2. Put startup/load logic into `InitializeAsync`.
3. Wrap long-running tasks with busy helpers.
4. Derive the page from `PageBase<TViewModel>` (or root shell from `ShellBase<TViewModel>`).

## Example

```csharp
public partial class WorkOrderListViewModel : BaseViewModel
{
    public override async Task InitializeAsync()
    {
        await IsBusyFor(async () =>
        {
            // load data here
            await Task.Delay(100);
        });
    }
}
```

## Best Practices

- Keep `InitializeAsync` idempotent.
- Avoid direct UI APIs in view models.
- Use constructor injection for dependencies.
