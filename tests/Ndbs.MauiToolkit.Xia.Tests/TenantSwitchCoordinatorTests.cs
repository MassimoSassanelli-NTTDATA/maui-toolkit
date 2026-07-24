using CommunityToolkit.Mvvm.Messaging;
using NDBS.Xia.Api.Dtos;
using NSubstitute;
using Ndbs.MauiToolkit.Context;
using Ndbs.MauiToolkit.Xia.Context;
using Ndbs.MauiToolkit.Xia.Data;
using Ndbs.MauiToolkit.Xia.Messaging;
using Ndbs.MauiToolkit.Xia.Navigation;
using Ndbs.MauiToolkit.Xia.Startup;
using Ndbs.MauiToolkit.Xia.Switching;
using Ndbs.MauiToolkit.Xia.Sync;
using Ndbs.MauiToolkit.Xia.Workspace;
using Xunit;

namespace Ndbs.MauiToolkit.Xia.Tests;

public sealed class TenantSwitchCoordinatorTests
{
    private const string UserId = "user-1";

    private readonly ITenantContext _tenantContext = Substitute.For<ITenantContext>();
    private readonly IUserContext _userContext = Substitute.For<IUserContext>();
    private readonly ITenantSwitchGuard _guard = Substitute.For<ITenantSwitchGuard>();
    private readonly ISyncService _sync = Substitute.For<ISyncService>();
    private readonly ITenantScopedCache _cache = Substitute.For<ITenantScopedCache>();
    private readonly IDatabaseSessionManager _db = Substitute.For<IDatabaseSessionManager>();
    private readonly ITenantWorkspaceProvider _workspace = Substitute.For<ITenantWorkspaceProvider>();
    private readonly LastTenantStore _lastTenant = new(new InMemoryKeyValueStore());
    private readonly ITenantNavigator _navigator = Substitute.For<ITenantNavigator>();
    private readonly WeakReferenceMessenger _messenger = new();

    private static TenantDto Tenant(string name) => new() { Id = Guid.NewGuid(), Name = name };

    private static TenantWorkspace WorkspaceFor(Guid tenantId) => new()
    {
        UserId = UserId,
        TenantId = tenantId,
        TenantName = "tenant",
        RootPath = "/tmp/root",
        MaintenanceDbPath = "/tmp/root/maintenance.db",
        MicroAppDbPath = "/tmp/root/microapp.db",
        MetadataPath = "/tmp/root/tenant.json",
    };

    private TenantSwitchCoordinator CreateCoordinator()
    {
        _userContext.UserId.Returns(UserId);
        _guard.CanSwitchAsync(Arg.Any<TenantDto>(), Arg.Any<CancellationToken>())
            .Returns(TenantSwitchGuardResult.Allow());
        _workspace.GetOrCreateAsync(Arg.Any<string>(), Arg.Any<Guid>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(call => WorkspaceFor(call.ArgAt<Guid>(1)));

        return new TenantSwitchCoordinator(
            _tenantContext, _userContext, _guard, _sync, _cache, _db,
            _workspace, _lastTenant, _navigator, _messenger);
    }

    [Fact]
    public async Task SwitchTenantAsync_WhenTargetAlreadyActive_ReturnsNoChange()
    {
        var tenant = Tenant("A");
        _tenantContext.CurrentTenant.Returns(tenant);
        var coordinator = CreateCoordinator();

        var result = await coordinator.SwitchTenantAsync(tenant);

        Assert.Equal(TenantSwitchOutcome.NoChange, result.Outcome);
        await _guard.DidNotReceive().CanSwitchAsync(Arg.Any<TenantDto>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SwitchTenantAsync_WhenNoUser_IsBlocked()
    {
        _tenantContext.CurrentTenant.Returns((TenantDto?)null);
        _userContext.UserId.Returns((string?)null);
        var coordinator = new TenantSwitchCoordinator(
            _tenantContext, _userContext, _guard, _sync, _cache, _db,
            _workspace, _lastTenant, _navigator, _messenger);

        var result = await coordinator.SwitchTenantAsync(Tenant("A"));

        Assert.Equal(TenantSwitchOutcome.Blocked, result.Outcome);
        Assert.Equal(TenantSwitchBlockingReason.NotAuthenticated, result.BlockingReason);
    }

    [Fact]
    public async Task SwitchTenantAsync_WhenGuardBlocks_ReturnsBlockedWithReason()
    {
        _tenantContext.CurrentTenant.Returns((TenantDto?)null);
        _userContext.UserId.Returns(UserId);
        _guard.CanSwitchAsync(Arg.Any<TenantDto>(), Arg.Any<CancellationToken>())
            .Returns(TenantSwitchGuardResult.Block(TenantSwitchBlockingReason.UnsavedChanges, "unsaved"));
        var coordinator = new TenantSwitchCoordinator(
            _tenantContext, _userContext, _guard, _sync, _cache, _db,
            _workspace, _lastTenant, _navigator, _messenger);

        var result = await coordinator.SwitchTenantAsync(Tenant("A"));

        Assert.Equal(TenantSwitchOutcome.Blocked, result.Outcome);
        Assert.Equal(TenantSwitchBlockingReason.UnsavedChanges, result.BlockingReason);
        Assert.Equal("unsaved", result.Reason);
        await _db.DidNotReceive().CloseCurrentAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SwitchTenantAsync_Success_RunsOrchestrationAndPublishesMessages()
    {
        _tenantContext.CurrentTenant.Returns((TenantDto?)null);
        _lastTenant.ForceSelectionOnNextLogin = true;
        var coordinator = CreateCoordinator();
        var target = Tenant("B");

        TenantChangingMessage? changing = null;
        TenantChangedMessage? changed = null;
        var recipient = new object();
        _messenger.Register<TenantChangingMessage>(recipient, (_, m) => changing = m);
        _messenger.Register<TenantChangedMessage>(recipient, (_, m) => changed = m);

        var result = await coordinator.SwitchTenantAsync(target);

        Assert.Equal(TenantSwitchOutcome.Success, result.Outcome);
        Assert.Equal(target.Id, result.Tenant!.Id);

        // Context and preferences updated.
        await _tenantContext.Received(1).SetCurrentTenantAsync(target, Arg.Any<TenantWorkspace>(), Arg.Any<CancellationToken>());
        Assert.Equal(target.Id, await _lastTenant.GetLastTenantAsync(UserId));
        Assert.False(_lastTenant.ForceSelectionOnNextLogin);

        // Navigation reset and sync resumed for the target tenant.
        await _navigator.Received(1).NavigateToTenantHomeAsync(Arg.Any<CancellationToken>());
        await _sync.Received(1).ResumeForTenantAsync(target.Id, Arg.Any<CancellationToken>());

        // Messages published.
        Assert.NotNull(changing);
        Assert.Equal(target.Id, changing!.NewTenant.Id);
        Assert.NotNull(changed);
        Assert.Equal(target.Id, changed!.NewTenant.Id);
    }

    [Fact]
    public async Task SwitchTenantAsync_Success_TearsDownBeforeOpeningNewContext()
    {
        _tenantContext.CurrentTenant.Returns((TenantDto?)null);
        var coordinator = CreateCoordinator();
        var target = Tenant("B");

        await coordinator.SwitchTenantAsync(target);

        Received.InOrder(() =>
        {
            _sync.PauseAsync(Arg.Any<CancellationToken>());
            _cache.ClearAsync(Arg.Any<CancellationToken>());
            _db.CloseCurrentAsync(Arg.Any<CancellationToken>());
            _workspace.GetOrCreateAsync(UserId, target.Id, target.Name, Arg.Any<CancellationToken>());
            _tenantContext.SetCurrentTenantAsync(target, Arg.Any<TenantWorkspace>(), Arg.Any<CancellationToken>());
            _navigator.NavigateToTenantHomeAsync(Arg.Any<CancellationToken>());
            _sync.ResumeForTenantAsync(target.Id, Arg.Any<CancellationToken>());
        });
    }
}
