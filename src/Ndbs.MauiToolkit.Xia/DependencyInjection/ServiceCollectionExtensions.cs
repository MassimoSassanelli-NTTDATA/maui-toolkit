using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Maui.Networking;
using CommunityToolkit.Mvvm.Messaging;
using NDBS.Api.Handlers;
using NDBS.Xia.Api;
using Ndbs.MauiToolkit.Auth;
using Ndbs.MauiToolkit.DynamicTables;
using Ndbs.MauiToolkit.Xia.Context;
using Ndbs.MauiToolkit.Xia.Data;
using Ndbs.MauiToolkit.Xia.Initialization;
using Ndbs.MauiToolkit.Xia.MicroApps.Packaging;
using Ndbs.MauiToolkit.Xia.MicroApps.Persistence;
using Ndbs.MauiToolkit.Xia.Navigation;
using Ndbs.MauiToolkit.Xia.Startup;
using Ndbs.MauiToolkit.Xia.Switching;
using Ndbs.MauiToolkit.Xia.Sync;
using Ndbs.MauiToolkit.Xia.Workspace;
using Ndbs.MauiToolkit.Environments;
using Ndbs.MauiToolkit.Context;
using Ndbs.MauiToolkit.Workspace;
using Ndbs.MauiToolkit.Startup;

namespace Ndbs.MauiToolkit.Xia.DependencyInjection
{
    /// <summary>
    /// Dependency injection registration for the XIA-specific MAUI toolkit
    /// (tenant context, storage workspaces, tenant start-up and switching).
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers the XIA toolkit services (context, workspace, start-up resolver,
        /// tenant switch coordinator). Call this in
        /// MAUI startup after <c>UseMauiNdbsToolkit()</c>.
        /// </summary>
        /// <remarks>
        /// The infrastructure hooks (<see cref="ISyncService"/>,
        /// <see cref="IDatabaseSessionManager"/>, <see cref="ITenantScopedCache"/>,
        /// <see cref="ITenantNavigator"/> and <see cref="ITenantSwitchGuard"/>) are
        /// registered with safe defaults using <c>TryAdd</c>, so apps can override them
        /// by registering their own implementation beforehand.
        /// </remarks>
        /// <param name="services">The service collection.</param>
        /// <returns>The same service collection so calls can be chained.</returns>
        public static IServiceCollection UseMauiNdbsXia(this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);

            // Central context (single truth per app instance).
            services.TryAddSingleton<IUserContext, UserContext>();
            services.TryAddSingleton<ITenantContext, TenantContext>();

            // Local storage workspaces.
            services.TryAddSingleton<IAppDataRootProvider, MauiAppDataRootProvider>();
            services.TryAddSingleton<ITenantWorkspaceProvider, TenantWorkspaceProvider>();

            // Last-tenant preference and start-up decision.
            services.TryAddSingleton<IKeyValueStore, PreferencesKeyValueStore>();
            services.TryAddSingleton<ILastTenantStore, LastTenantStore>();
            services.TryAddSingleton<ITenantStartupResolver, TenantStartupResolver>();

            // Overridable infrastructure hooks (safe defaults until later steps).
            services.TryAddSingleton<ISyncService, NoOpSyncService>();
            services.TryAddSingleton<IDatabaseSessionManager, NoOpDatabaseSessionManager>();
            services.TryAddSingleton<ITenantScopedCache, NoOpTenantScopedCache>();
            services.TryAddSingleton<ITenantNavigator, NoOpTenantNavigator>();
            services.TryAddSingleton<ITenantSwitchGuard, DefaultTenantSwitchGuard>();

            // Messenger for loosely coupled tenant-change notifications.
            services.TryAddSingleton<IMessenger>(_ => WeakReferenceMessenger.Default);

            // Tenant switch orchestration.
            services.TryAddSingleton<ITenantSwitchCoordinator, TenantSwitchCoordinator>();

            // Context chain (User → Tenant → MicroApp), reacting to runtime changes.
            services.AddXiaContextChain();
            return services;
        }

        /// <summary>
        /// Registers the XIA context-chain infrastructure: the generic
        /// <see cref="Ndbs.MauiToolkit.Context.IContextChain"/>, the contexts owned by
        /// the XIA stages (<see cref="IMicroAppContext"/>) and the selection hand-off
        /// holders. It deliberately does <b>not</b> register any stage, because the
        /// chain composition and the stage order are an application concern: the app
        /// decides which contexts exist and in which order (this app is built around
        /// maintenance work orders; other apps have different business objects).
        /// </summary>
        /// <remarks>
        /// The app composes the chain with
        /// <see cref="Ndbs.MauiToolkit.Context.ContextChainServiceCollectionExtensions.AddUserContextStage"/>,
        /// <see cref="AddXiaTenantStage"/> and <see cref="AddXiaMicroAppStage"/>,
        /// passing its own order values. This method is idempotent and is called by
        /// <see cref="UseMauiNdbsXia(IServiceCollection)"/>.
        /// </remarks>
        /// <param name="services">The service collection.</param>
        /// <returns>The same service collection so calls can be chained.</returns>
        public static IServiceCollection AddXiaContextChain(this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);

            // Contexts owned by the stages.
            services.TryAddSingleton<IMicroAppContext, MicroAppContext>();

            // Selection / configuration hand-off holders (shared singletons).
            services.TryAddSingleton<XiaTenantConfiguration>();
            services.TryAddSingleton<TenantSelection>();
            services.TryAddSingleton<MicroAppSelection>();

            // The generic chain, composed from the stages the app registers.
            services.AddContextChain();

            return services;
        }

        /// <summary>
        /// Registers the XIA <see cref="TenantContextStage"/> at the chain position
        /// defined by the app.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="order">The position of the tenant stage in the chain.</param>
        /// <returns>The same service collection so calls can be chained.</returns>
        public static IServiceCollection AddXiaTenantStage(this IServiceCollection services, int order)
        {
            ArgumentNullException.ThrowIfNull(services);
            return services.AddContextStage(sp => new TenantContextStage(
                sp.GetRequiredService<IUserContext>(),
                sp.GetRequiredService<ITenantContext>(),
                sp.GetRequiredService<ITenantSwitchCoordinator>(),
                sp.GetRequiredService<TenantSelection>(),
                sp.GetRequiredService<XiaTenantConfiguration>(),
                order));
        }

        /// <summary>
        /// Registers the XIA <see cref="MicroAppContextStage"/> at the chain position
        /// defined by the app.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="order">The position of the micro-app stage in the chain.</param>
        /// <returns>The same service collection so calls can be chained.</returns>
        public static IServiceCollection AddXiaMicroAppStage(this IServiceCollection services, int order)
        {
            ArgumentNullException.ThrowIfNull(services);
            return services.AddContextStage(sp => new MicroAppContextStage(
                sp.GetRequiredService<IUserContext>(),
                sp.GetRequiredService<ITenantContext>(),
                sp.GetRequiredService<IMicroAppContext>(),
                sp.GetRequiredService<ITenantWorkspaceProvider>(),
                sp.GetRequiredService<MicroAppSelection>(),
                order));
        }

        /// <summary>
        /// Registers the micro app synchronization (specification
        /// <c>docs/specs/MicroApp_Synchronisierung_Fachlich.md</c>, §1–7): the dedicated
        /// <c>microapp.db</c> EF Core context, its repository, the ZIP package service,
        /// the dynamic-table import targeting <c>microapp.db</c> and the real
        /// <see cref="ISyncService"/> implementation
        /// (<see cref="MicroAppSyncService"/>), replacing the <c>NoOpSyncService</c>
        /// default. Call this after <see cref="UseMauiNdbsXia(IServiceCollection)"/>.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configureCsv">Optional configuration of the CSV import options.</param>
        /// <returns>The same service collection so calls can be chained.</returns>
        public static IServiceCollection AddXiaMicroAppSync(
            this IServiceCollection services,
            Action<CsvImportOptions>? configureCsv = null)
        {
            ArgumentNullException.ThrowIfNull(services);

            // The micro app database lives in the active tenant workspace
            // (.../{TenantName}/microapp.db) and can therefore only be opened after a
            // tenant has been activated (context chain), analogous to the app database.
            services.AddDbContext<MicroAppDbContext>((sp, options) =>
            {
                var tenantContext = sp.GetRequiredService<ITenantContext>();
                var workspace = tenantContext.CurrentWorkspace
                    ?? throw new InvalidOperationException(
                        "No active tenant: the micro app database can only be opened after the tenant has been selected.");

                options.UseSqlite($"Filename={workspace.MicroAppDbPath}");
            });

            services.AddScoped<IMicroAppRepository, EfMicroAppRepository>();

            // The dynamic content tables (§5.4) are created in the same microapp.db.
            services.AddDynamicTables<MicroAppDbContext>(configureCsv);

            // Package extraction/manifest reading and connectivity probing.
            services.TryAddSingleton<IMicroAppPackageService, MicroAppPackageService>();
            services.TryAddSingleton<IConnectivity>(_ => Connectivity.Current);

            // Replace the NoOpSyncService default with the real implementation.
            services.RemoveAll<ISyncService>();
            services.AddSingleton<ISyncService, MicroAppSyncService>();

            return services;
        }


        /// <param name="services">The service collection.</param>
        /// <param name="configureApi">Configures the XIA API options (for example the base URI).</param>
        /// <returns>The same service collection so calls can be chained.</returns>
        public static IServiceCollection UseMauiNdbsXia(this IServiceCollection services, Action<XiaApiOption> configureApi)
        {
            ArgumentNullException.ThrowIfNull(services);
            ArgumentNullException.ThrowIfNull(configureApi);

            services.UseMauiNdbsXia();

            var httpBuilder = services.AddXiaApi(configureApi);

            // Provide the bearer token for XIA API calls from the existing auth layer.
            services.TryAddSingleton<IApiTokenProvider<IXiaRefitApi>>(sp =>
                new ApiTokenProvider<IXiaRefitApi>(
                    () => sp.GetRequiredService<IAuthenticationService>().GetAccessTokenAsync()));
            services.TryAddTransient<BearerAuthHeaderHandler<IXiaRefitApi>>();
            httpBuilder.AddHttpMessageHandler<BearerAuthHeaderHandler<IXiaRefitApi>>();

            return services;
        }

        /// <summary>
        /// Registers the generic system-environment infrastructure (schema-less store,
        /// startup resolver, switch coordinator, provisioning and validator). Call this
        /// after <see cref="UseMauiNdbsXia(IServiceCollection)"/> and register the
        /// concrete environment components the app is composed of via
        /// <see cref="AddEnvironmentComponent{TComponent}"/>.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <returns>The same service collection so calls can be chained.</returns>
        public static IServiceCollection UseMauiNdbsSystemEnvironments(this IServiceCollection services)
        {
            ArgumentNullException.ThrowIfNull(services);

            // Shared infrastructure (also registered by UseMauiNdbsXia; TryAdd keeps
            // this method usable on its own and idempotent).
            services.TryAddSingleton<IAppDataRootProvider, MauiAppDataRootProvider>();
            services.TryAddSingleton<IKeyValueStore, PreferencesKeyValueStore>();
            services.TryAddSingleton<IUserContext, UserContext>();
            services.TryAddSingleton<IMessenger>(_ => WeakReferenceMessenger.Default);

            services.TryAddSingleton<ISystemEnvironmentStore, JsonFileSystemEnvironmentStore>();
            services.TryAddSingleton<SystemEnvironmentValidator>();
            services.TryAddSingleton<ISystemEnvironmentStartupResolver, SystemEnvironmentStartupResolver>();
            services.TryAddSingleton<ISystemEnvironmentSwitchCoordinator, SystemEnvironmentSwitchCoordinator>();
            services.TryAddSingleton<ISystemEnvironmentProvisioningService, SystemEnvironmentProvisioningService>();

            return services;
        }

        /// <summary>
        /// Registers a system-environment component. Apps compose their environment
        /// model by registering exactly the components they support (for example an
        /// identity-provider component and API components).
        /// </summary>
        /// <typeparam name="TComponent">The component implementation type.</typeparam>
        /// <param name="services">The service collection.</param>
        /// <returns>The same service collection so calls can be chained.</returns>
        public static IServiceCollection AddEnvironmentComponent<TComponent>(this IServiceCollection services)
            where TComponent : class, ISystemEnvironmentComponent
        {
            ArgumentNullException.ThrowIfNull(services);
            services.AddSingleton<ISystemEnvironmentComponent, TComponent>();
            return services;
        }
    }
}
