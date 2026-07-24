# Event Journal

The optional `Ndbs.MauiToolkit.EventJournal` assembly provides a curated,
platform-neutral (`net10.0`) event journal for technical and operational events.
It is designed for bounded diagnostics, not as an unbounded analytics store.

## What It Solves

Use this feature when you need to:

- record structured runtime events with correlation IDs and outcomes
- enrich event context in a centralized way
- sanitize or truncate payloads before persisting
- write events to multiple sinks (for example logger and SQLite)
- enforce retention by age and row count

## Key Types

- `IEventJournal`: primary write API (`WriteAsync`, `BeginScope`)
- `EventJournal`: default implementation with gating, enrichment, sanitization, and sink fan-out
- `EventJournalOptions`: governance options (`Enabled`, `MaxPayloadBytes`, `RetentionDays`, `MaxRows`, `CaptureExceptions`, `AllowedCategories`)
- `IEventSink`: sink abstraction
- `LoggerEventSink`: logs journal events through `ILogger`
- `SqliteEventSink`: persists to SQLite using host-provided path
- `IEventJournalPathProvider`: host callback for SQLite file location
- `IEventPayloadSanitizer`: payload sanitization abstraction
- `DefaultEventPayloadSanitizer`: default sanitizer implementation
- `IEventEnricher`: context enrichment abstraction
- `EventJournalExtensions.RecordAsync`: convenience API for point-in-time events

## DI Registration

```csharp
builder.Services
    .AddEventJournal(options =>
    {
        options.RetentionDays = 14;
        options.MaxRows = 10_000;
        options.AllowedCategories.Add("sync");
    })
    .AddEventJournalDatabasePath(sp =>
    {
        // Return null to skip SQLite persistence temporarily
        return Path.Combine(FileSystem.AppDataDirectory, "event-journal.db");
    });
```

Notes:

- `AddEventJournal` registers the journal, default sanitizer, and selected sinks.
- `AddEventJournalDatabasePath` is required for SQLite persistence.
- If path resolution returns `null` or empty, SQLite writes are skipped.

## Typical Usage

```csharp
await eventJournal.RecordAsync(
    category: "sync",
    name: "orders-pull",
    outcome: EventOutcome.Succeeded,
    payloadJson: "{\"batch\":42}",
    errorCode: null,
    exception: null,
    cancellationToken: ct);
```

Scoped timing usage:

```csharp
await using var scope = eventJournal.BeginScope("sync", "orders-pull");
// ... execute operation ...
await scope.CompleteAsync(EventOutcome.Succeeded, cancellationToken: ct);
```

## Boundaries

- Keep this journal focused on technical/operational events.
- Do not store sensitive raw payload data; sanitize first.
- Do not use this as domain-event sourcing or BI/analytics infrastructure.
