# Icons

## Purpose

Provides a platform icon abstraction that maps common icon keys to native icon systems.

Core types:

- `PlatformIconView`
- `AppIconCatalog`
- `IPlatformIconResolver` and `PlatformIconResolver`
- `AddNdbsPlatformIconHandlers`

## When to Use

Use for consistent icon usage across iOS, Android, and Windows without per-page platform checks.

## How to Use

1. Register toolkit services and icon handlers.
2. Use icon keys from `AppIconCatalog`.
3. Bind `PlatformIconView` in XAML.

## Example

```csharp
builder.ConfigureMauiHandlers(h => h.AddNdbsPlatformIconHandlers());
```

```xml
<icons:PlatformIconView IconKey="WorkOrder" Size="20" />
```

## Best Practices

- Keep icon keys semantic (action/domain meaning) instead of platform-oriented.
- Define color via theme resources.
- Reuse catalog keys to avoid duplicate icon mappings.
