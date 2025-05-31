using BananaPoolLocker.Models;
using BananaPoolLocker.Services;

namespace BananaPoolLocker.Tests;

public class UnitTest1
{
    private const string ConnectionString = "UseDevelopmentStorage=true";
    private const string TableName = "ResourcePoolTestTable";

    private readonly ResourcePoolService _resourcePoolService = new ResourcePoolService(ConnectionString, TableName);

    [Fact]
    public async Task Should_Add_And_Allocate_Resource()
    {
        var resource = new ResourceEntity();

        await _resourcePoolService.AddResourceAsync(resource);

        var allocatedResource = await _resourcePoolService.TryAcquireResourceAsync(resource.RowKey);

        Assert.NotNull(allocatedResource);
        Assert.Equal(ResourceEntityState.InUse, allocatedResource.Status);
    }

    [Fact]
    public async Task Should_Not_Allocate_Resource_Twice()
    {
        var resource = new ResourceEntity();
        await _resourcePoolService.AddResourceAsync(resource);

        // First allocation should succeed
        var firstAllocation = await _resourcePoolService.TryAcquireResourceAsync(resource.RowKey);
        Assert.NotNull(firstAllocation);
        Assert.Equal(ResourceEntityState.InUse, firstAllocation.Status);

        // Attempt to allocate the same resource again
        var secondAllocation = await _resourcePoolService.TryAcquireResourceAsync(resource.RowKey);
        Assert.Null(secondAllocation); // Should return null since the resource is already in use
    }

    [Fact]
    public async Task Should_Handle_Concurrent_Allocatiion()
    {
        var resource = new ResourceEntity();
        await _resourcePoolService.AddResourceAsync(resource);

        // Simulate concurrent allocation attempts
        var firstTask = _resourcePoolService.TryAcquireResourceAsync(resource.RowKey);
        var secondTask = _resourcePoolService.TryAcquireResourceAsync(resource.RowKey);

        var results = await Task.WhenAll(firstTask, secondTask);

        // One should succeed and the other should fail
        Assert.Equal(1, results.Count(r => r != null));
        Assert.Equal(ResourceEntityState.InUse, results.FirstOrDefault(r => r != null)?.Status);
    }
}
