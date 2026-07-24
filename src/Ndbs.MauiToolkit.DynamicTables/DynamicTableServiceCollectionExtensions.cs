using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Ndbs.MauiToolkit.DynamicTables
{
    public static class DynamicTableServiceCollectionExtensions
    {
        public static IServiceCollection AddDynamicTables<TContext>(
            this IServiceCollection services,
            Action<CsvImportOptions>? configure = null)
            where TContext : DbContext
        {
            var options = new CsvImportOptions();
            configure?.Invoke(options);
            services.AddSingleton(options);

            // Repository nutzt den BESTEHENDEN, bereits registrierten App-Context.
            // Scoped, damit es der Lebensdauer des DbContext folgt.
            services.AddScoped<IDynamicTableRepository>(sp =>
                new DynamicTableRepository(sp.GetRequiredService<TContext>()));

            services.AddScoped<IDynamicTableMemoryCache>(sp =>
                new DynamicTableMemoryCache(sp.GetRequiredService<IDynamicTableRepository>()));

            services.AddScoped<IDynamicTableImportService, DynamicTableImportService>();

            return services;
        }
    }
}
