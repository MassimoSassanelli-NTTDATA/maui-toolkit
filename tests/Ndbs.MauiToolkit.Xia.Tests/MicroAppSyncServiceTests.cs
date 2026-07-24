using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Networking;
using NDBS.Xia.Api;
using NDBS.Xia.Api.Dtos;
using NSubstitute;
using Ndbs.MauiToolkit.Xia.Context;
using Ndbs.MauiToolkit.Xia.MicroApps.Packaging;
using Ndbs.MauiToolkit.Xia.MicroApps.Sync;
using Ndbs.MauiToolkit.Xia.Sync;
using Ndbs.MauiToolkit.Xia.Workspace;
using Xunit;

namespace Ndbs.MauiToolkit.Xia.Tests;

public sealed class MicroAppSyncServiceTests
{
    private readonly IServiceScopeFactory _scopeFactory = Substitute.For<IServiceScopeFactory>();
    private readonly IXiaApiService _api = Substitute.For<IXiaApiService>();
    private readonly IConnectivity _connectivity = Substitute.For<IConnectivity>();
    private readonly ITenantContext _tenantContext = Substitute.For<ITenantContext>();
    private readonly ITenantWorkspaceProvider _workspaceProvider = Substitute.For<ITenantWorkspaceProvider>();
    private readonly IMicroAppPackageService _packageService = Substitute.For<IMicroAppPackageService>();
    private readonly ILogger<MicroAppSyncService> _logger = Substitute.For<ILogger<MicroAppSyncService>>();

    private MicroAppSyncService CreateService() => new(
        _scopeFactory, _api, _connectivity, _tenantContext, _workspaceProvider, _packageService, _logger);

    [Fact]
    public async Task SynchronizeMicroAppsAsync_Offline_SkipsWithoutApiCalls()
    {
        _connectivity.NetworkAccess.Returns(NetworkAccess.None);
        var progress = new RecordingProgress();

        await CreateService().SynchronizeMicroAppsAsync(progress: progress);

        await _api.DidNotReceive().GetMyMicroAppsMetaAsync(Arg.Any<Guid>());
        await _api.DidNotReceive().GetMyMicroAppsEtagsAsync(Arg.Any<Guid>());
        _scopeFactory.DidNotReceive().CreateScope();
        Assert.Contains(progress.Reports, r => r.StatusText.Contains("Offline", StringComparison.Ordinal));
    }

    [Fact]
    public async Task SynchronizeMicroAppsAsync_NoActiveTenant_SkipsWithoutApiCalls()
    {
        _connectivity.NetworkAccess.Returns(NetworkAccess.Internet);
        _tenantContext.CurrentTenant.Returns((TenantDto?)null);
        _tenantContext.CurrentWorkspace.Returns((TenantWorkspace?)null);

        await CreateService().SynchronizeMicroAppsAsync();

        await _api.DidNotReceive().GetMyMicroAppsMetaAsync(Arg.Any<Guid>());
    }

    private sealed class RecordingProgress : IProgress<MicroAppSyncProgress>
    {
        public List<MicroAppSyncProgress> Reports { get; } = new();

        public void Report(MicroAppSyncProgress value) => Reports.Add(value);
    }
}
