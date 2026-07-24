using Ndbs.MauiToolkit.Workspace;
using Ndbs.MauiToolkit.Xia.Workspace;
using Xunit;

namespace Ndbs.MauiToolkit.Xia.Tests;

public sealed class TenantWorkspaceProviderTests : IDisposable
{
    private readonly string _root;
    private readonly TenantWorkspaceProvider _provider;

    public TenantWorkspaceProviderTests()
    {
        _root = Path.Combine(Path.GetTempPath(), "xia-tests-" + Guid.NewGuid().ToString("N"));
        _provider = new TenantWorkspaceProvider(new FixedRootProvider(_root));
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, recursive: true);
        }
    }

    [Fact]
    public async Task GetOrCreateAsync_CreatesTenantFolderLayout()
    {
        var tenantId = Guid.NewGuid();

        var workspace = await _provider.GetOrCreateAsync("user-1", tenantId, "Contoso");

        Assert.Equal("user-1", workspace.UserId);
        Assert.Equal(tenantId, workspace.TenantId);
        Assert.Equal("Contoso", workspace.TenantName);
        Assert.Equal(Path.Combine(_root, "user-1", "Contoso"), workspace.RootPath);
        Assert.True(Directory.Exists(workspace.RootPath));
        Assert.Equal(Path.Combine(workspace.RootPath, "maintenance.db"), workspace.MaintenanceDbPath);
        Assert.Equal(Path.Combine(workspace.RootPath, "microapp.db"), workspace.MicroAppDbPath);
        Assert.Equal(Path.Combine(workspace.RootPath, "tenant.json"), workspace.MetadataPath);
        Assert.Same(workspace, _provider.CurrentWorkspace);
    }

    [Fact]
    public async Task GetOrCreateMicroAppAsync_CreatesMicroAppFolder()
    {
        var tenantId = Guid.NewGuid();

        var workspace = await _provider.GetOrCreateMicroAppAsync("user-1", tenantId, "Contoso", "wartung");

        var expectedRoot = Path.Combine(_root, "user-1", "Contoso", "wartung");
        Assert.Equal("wartung", workspace.MicroAppName);
        Assert.Equal(expectedRoot, workspace.RootPath);
        Assert.True(Directory.Exists(workspace.RootPath));
    }

    [Fact]
    public async Task GetOrCreateFolderAsync_CreatesNamedFolder()
    {
        var tenantId = Guid.NewGuid();

        var workspace = await _provider.GetOrCreateFolderAsync("user-1", tenantId, "Contoso", "AU-4711");

        var expectedRoot = Path.Combine(_root, "user-1", "Contoso", "AU-4711");
        Assert.Equal("AU-4711", workspace.FolderName);
        Assert.Equal(expectedRoot, workspace.RootPath);
        Assert.True(Directory.Exists(workspace.RootPath));
    }

    [Fact]
    public async Task InvalidCharactersInNames_AreSanitized()
    {
        var tenantId = Guid.NewGuid();

        var tenant = await _provider.GetOrCreateAsync("user-1", tenantId, "Con/to:so");
        var folder = await _provider.GetOrCreateFolderAsync("user-1", tenantId, "Con/to:so", "AU/47:11");

        Assert.Equal("Con_to_so", tenant.TenantName);
        Assert.Equal("AU_47_11", folder.FolderName);
        Assert.True(Directory.Exists(tenant.RootPath));
        Assert.True(Directory.Exists(folder.RootPath));
    }

    [Fact]
    public async Task DifferentTenants_AreStoredInSeparateFolders()
    {
        var a = await _provider.GetOrCreateAsync("user-1", Guid.NewGuid(), "Alpha");
        var b = await _provider.GetOrCreateAsync("user-1", Guid.NewGuid(), "Beta");

        Assert.NotEqual(a.RootPath, b.RootPath);
    }

    private sealed class FixedRootProvider : IAppDataRootProvider
    {
        private readonly string _root;
        public FixedRootProvider(string root) => _root = root;
        public string GetAppDataRoot() => _root;
    }
}
