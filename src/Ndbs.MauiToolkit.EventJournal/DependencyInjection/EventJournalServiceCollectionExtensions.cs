using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Ndbs.MauiToolkit.EventJournal.Abstractions;
using Ndbs.MauiToolkit.EventJournal.Journaling;
using Ndbs.MauiToolkit.EventJournal.Options;
using Ndbs.MauiToolkit.EventJournal.Persistence;
using Ndbs.MauiToolkit.EventJournal.Sanitization;
using Ndbs.MauiToolkit.EventJournal.Sinks;

namespace Ndbs.MauiToolkit.EventJournal.DependencyInjection
{
    /// <summary>
    /// Dependency injection registration for the generic event journal.
    /// </summary>
    public static class EventJournalServiceCollectionExtensions
    {
        /// <summary>
        /// Registers the event journal (<see cref="IEventJournal"/>), the default
        /// payload sanitizer and the requested sinks. The logger sink is safe to
        /// enable unconditionally; the SQLite sink additionally requires an
        /// <see cref="IEventJournalPathProvider"/> (register one via
        /// <see cref="AddEventJournalDatabasePath"/>).
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configure">Optional configuration of the governance options.</param>
        /// <param name="addLoggerSink">Registers the <see cref="LoggerEventSink"/>. Defaults to <see langword="true"/>.</param>
        /// <param name="addSqliteSink">Registers the <see cref="SqliteEventSink"/>. Defaults to <see langword="true"/>.</param>
        /// <returns>The same service collection so calls can be chained.</returns>
        public static IServiceCollection AddEventJournal(
            this IServiceCollection services,
            Action<EventJournalOptions>? configure = null,
            bool addLoggerSink = true,
            bool addSqliteSink = true)
        {
            ArgumentNullException.ThrowIfNull(services);

            var options = new EventJournalOptions();
            configure?.Invoke(options);
            services.TryAddSingleton(options);

            services.TryAddSingleton<TimeProvider>(TimeProvider.System);
            services.TryAddSingleton<IEventPayloadSanitizer, DefaultEventPayloadSanitizer>();
            services.TryAddSingleton<IEventJournal, Journaling.EventJournal>();

            if (addLoggerSink)
            {
                services.AddSingleton<IEventSink, LoggerEventSink>();
            }

            if (addSqliteSink)
            {
                services.AddSingleton<IEventSink, SqliteEventSink>();
            }

            return services;
        }

        /// <summary>
        /// Registers a delegate-based <see cref="IEventJournalPathProvider"/> that
        /// resolves the SQLite database path at runtime (for example from the active
        /// tenant workspace). Returning <see langword="null"/> skips persistence.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="resolvePath">Resolves the database path from the service provider.</param>
        /// <returns>The same service collection so calls can be chained.</returns>
        public static IServiceCollection AddEventJournalDatabasePath(
            this IServiceCollection services,
            Func<IServiceProvider, string?> resolvePath)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(resolvePath);

            services.TryAddSingleton<IEventJournalPathProvider>(sp =>
                new DelegateEventJournalPathProvider(() => resolvePath(sp)));

            return services;
        }
    }
}
