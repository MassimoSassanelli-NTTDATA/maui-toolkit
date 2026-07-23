using Microsoft.Extensions.DependencyInjection;
using Ndbs.MauiToolkit.Icons;
using Ndbs.MauiToolkit.Services;

namespace Ndbs.MauiToolkit
{
    /// <summary>
    /// Provides dependency injection registration extensions for NDBS MauiToolkit services.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers the default NDBS MauiToolkit services in the specified service collection.
        /// </summary>
        /// <param name="services">The service collection to add toolkit services to.</param>
        /// <returns>The same service collection so calls can be chained.</returns>
        public static IServiceCollection UseMauiNdbsToolkit(this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);

            services.AddSingleton<INavigationService, NavigationService>();
            services.AddSingleton<IDialogService, DialogService>();

            foreach (var icon in AppIconCatalog.All)
                services.AddSingleton(icon);

            services.AddSingleton<IPlatformIconResolver, PlatformIconResolver>();

            return services;
        }
    }
}