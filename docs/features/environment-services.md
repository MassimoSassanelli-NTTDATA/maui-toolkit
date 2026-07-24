# Environment Services

## Purpose

Provides schema-less environment modeling, provisioning, startup resolution, validation, and controlled environment switching.

Core types:

- `SystemEnvironment`
- `ISystemEnvironmentStore` and `JsonFileSystemEnvironmentStore`
- `ISystemEnvironmentProvisioningService`
- `ISystemEnvironmentStartupResolver`
- `ISystemEnvironmentSwitchCoordinator`
- `ISystemEnvironmentComponent`
- `EnvironmentSwitchOptions`

## When to Use

Use when the app supports multiple backend/identity environments or onboarding via JSON/QR data.

## How to Use

1. Persist and load environments through `ISystemEnvironmentStore`.
2. Validate/provision environments with provisioning services.
3. Resolve startup environment with startup resolver.
4. Switch via switch coordinator to guarantee controlled transition.

## Example Flow

```text
Provision -> Validate -> Persist -> Resolve Startup -> Switch
```

## Best Practices

- Validate before persisting.
- Keep switch operations atomic from the app perspective.
- Notify interested components via messaging after successful switch.
- Use `SwitchAsync(target, EnvironmentSwitchOptions)` to decide per switch whether
  session-scoped state is reset. The default (`SwitchAsync(target)`) performs a full
  tear-down (`ResetActiveSession = true`); pass `ResetActiveSession = false` to keep
  an active session across the switch. The core stays auth-agnostic — identity-provider
  components interpret the intent (for example as "sign out or keep signed in").
