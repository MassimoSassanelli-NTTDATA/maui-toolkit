using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Ndbs.MauiToolkit.Context
{
    /// <summary>
    /// Dependency injection helpers for the generic context chain framework.
    /// </summary>
    public static class ContextChainServiceCollectionExtensions
    {
        /// <summary>
        /// Registers the <see cref="IContextChain"/> as a singleton. The chain is
        /// composed from every <see cref="IContextStage"/> registered in the
        /// container, so register the stages (via <see cref="AddContextStage{TStage}"/>
        /// or directly) before resolving the chain.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <returns>The same service collection so calls can be chained.</returns>
        public static IServiceCollection AddContextChain(this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);
            services.TryAddSingleton<IContextChain>(sp => new ContextChain(sp.GetServices<IContextStage>()));
            return services;
        }

        /// <summary>
        /// Registers a context stage. Stages are collected into the
        /// <see cref="IContextChain"/> in the order defined by their
        /// <see cref="IContextStage.Order"/>. Registering the same implementation type
        /// more than once is a no-op, so this is safe to call from idempotent setup code.
        /// </summary>
        /// <typeparam name="TStage">The stage implementation type.</typeparam>
        /// <param name="services">The service collection.</param>
        /// <returns>The same service collection so calls can be chained.</returns>
        public static IServiceCollection AddContextStage<TStage>(this IServiceCollection services)
            where TStage : class, IContextStage
        {
            ArgumentNullException.ThrowIfNull(services);
            services.TryAddEnumerable(ServiceDescriptor.Singleton<IContextStage, TStage>());
            return services;
        }

        /// <summary>
        /// Registers a context stage created by a factory. Use this overload when the
        /// stage needs values that are only known at composition time – most notably
        /// its <see cref="IContextStage.Order"/>, which is an application concern. The
        /// caller is responsible for registering each stage exactly once.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="factory">Creates the stage from the service provider.</param>
        /// <returns>The same service collection so calls can be chained.</returns>
        public static IServiceCollection AddContextStage(
            this IServiceCollection services,
            Func<IServiceProvider, IContextStage> factory)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(factory);
            services.AddSingleton(factory);
            return services;
        }

        /// <summary>
        /// Registers the application-agnostic <see cref="UserContextStage"/> at the
        /// chain position defined by the app.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="order">The position of the user stage in the chain.</param>
        /// <returns>The same service collection so calls can be chained.</returns>
        public static IServiceCollection AddUserContextStage(this IServiceCollection services, int order)
        {
            ArgumentNullException.ThrowIfNull(services);
            return services.AddContextStage(sp => new UserContextStage(sp.GetRequiredService<IUserContext>(), order));
        }
    }
}
