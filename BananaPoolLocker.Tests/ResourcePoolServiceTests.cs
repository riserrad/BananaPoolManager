using BananaPoolLocker.Models;
using BananaPoolLocker.Services;

namespace BananaPoolLocker.Tests;

public class ResourcePoolServiceTests
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

    [Fact]
    public async Task Should_Refill_Resources_If_Below_Threshold()
    {
        await _resourcePoolService.RefillPoolAsync();

        var availableResources = await _resourcePoolService.GetAvailableResourcesAsync();

        var countOfResourcesToAllocate = availableResources.Count - _resourcePoolService.MinimumResources + 1;
        for (int i = 0; i < countOfResourcesToAllocate; i++)
        {
            var resource = availableResources[i];
            await _resourcePoolService.TryAcquireResourceAsync(resource.RowKey);
        }

        availableResources = await _resourcePoolService.GetAvailableResourcesAsync();
        Assert.True(availableResources.Count >= _resourcePoolService.MinimumResources, "Available resources should not be below the threshold after allocation.");
    }

    [Fact]
    public async Task Should_Assign_Random_Resource()
    {
        var resource = await _resourcePoolService.AllocateRandomResourceAsync();
        Assert.NotNull(resource);
    }

    [Fact]
    public async Task Should_Allocate_Unique_Resources_For_Concurrent_Requests()
    {
        var tasks = Enumerable.Range(0, 10).Select(_ => _resourcePoolService.AllocateRandomResourceAsync()).ToArray();
        var results = await Task.WhenAll(tasks);

        // Ensure all allocated resources are unique
        Assert.Equal(results.Distinct().Count(), results.Length);
    }

    [Fact]
    public async Task Should_Delete_Resource()
    {
        var resource = new ResourceEntity();
        await _resourcePoolService.AddResourceAsync(resource);

        await _resourcePoolService.DeleteResourceAsync(resource.RowKey);

        var deletedResource = await _resourcePoolService.GetResourceAsync(resource.RowKey);
        Assert.Null(deletedResource);
    }

    [Fact]
    public async Task Should_Silently_Fail_If_Deleting_NonExistent_Resource()
    {
        // Attempt to delete a resource that does not exist
        await _resourcePoolService.DeleteResourceAsync("non-existent-row-key");

        // No exception should be thrown, and the method should complete successfully
        Assert.True(true);
    }
}
