# Navigation And Dialogs

## Purpose

Abstracts navigation and dialogs from UI classes so view models remain testable and platform-agnostic.

Core types:

- `INavigationService`
- `IDialogService`
- `NavigationService`
- `DialogService`

## When to Use

Use for route navigation, back navigation, confirm dialogs, and alerts from view models.

## How to Use

1. Register toolkit services: `builder.Services.UseMauiNdbsToolkit();`
2. Inject `INavigationService` and/or `IDialogService` into your view model.
3. Use service methods instead of `Shell.Current` and `DisplayAlert` directly.

## Example

```csharp
public sealed class DetailViewModel
{
    private readonly INavigationService _navigation;
    private readonly IDialogService _dialogs;

    public DetailViewModel(INavigationService navigation, IDialogService dialogs)
    {
        _navigation = navigation;
        _dialogs = dialogs;
    }

    public async Task DeleteAsync()
    {
        var confirmed = await _dialogs.ConfirmAsync("Delete", "Delete item?", "Yes", "No");
        if (!confirmed) return;

        // delete logic
        await _navigation.GoBackAsync();
    }
}
```

## Best Practices

- Keep route names centralized and stable.
- Pass structured parameters instead of loosely formatted strings.
- Keep user messaging in dialog calls short and explicit.
