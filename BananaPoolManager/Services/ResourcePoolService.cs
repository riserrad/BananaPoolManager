using System;
using Azure;
using Azure.Data.Tables;
using BananaPoolManager.Models;

namespace BananaPoolManager.Services;

public class ResourcePoolService
{
    public readonly TableClient _tableClient;
    public readonly int MinimumResources = 10; // Minimum number of resources to maintain in the pool
    public readonly int Buffer = 2; // Optional buffer to ensure we have enough resources

    public ResourcePoolService(string connectionString, string tableName)
    {
        _tableClient = new TableClient(connectionString, tableName);
        _tableClient.CreateIfNotExists();
    }

    public async Task<List<ResourceEntity>> GetAllResourcesAsync()
    {
        var resources = _tableClient.QueryAsync<ResourceEntity>(filter: $"PartitionKey eq 'ResourcePool'");
        return await ToListAsync<ResourceEntity>(resources);
    }

    public async Task<List<ResourceEntity>> GetAvailableResourcesAsync()
    {
        var resources = _tableClient.QueryAsync<ResourceEntity>(
            filter: $"PartitionKey eq 'ResourcePool' and Status eq '{ResourceEntityState.Available}'");

        return await ToListAsync<ResourceEntity>(resources);
    }

    public async Task<ResourceEntity?> TryAcquireResourceAsync(string rowKey)
    {
        try
        {
            var resource = await _tableClient.GetEntityAsync<ResourceEntity>("ResourcePool", rowKey);
            if (resource.Value.Status == ResourceEntityState.Available)
            {
                resource.Value.Status = ResourceEntityState.InUse;
                // Here is where the lock happens. If another process had updated the resource,
                // the ETag will not match, and the update will fail with a 412 Precondition Failed error.
                // This ensures that only one process can acquire the resource at a time.
                await _tableClient.UpdateEntityAsync(resource.Value, resource.Value.ETag, TableUpdateMode.Replace);
                await RefillPoolAsync(); // Check and refill the pool if needed
                return resource.Value;
            }
            return null; // Resource is not available
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return null; // Resource not found
        }
        catch (RequestFailedException ex) when (ex.Status == 412)
        {
            // This means the resource was updated by another process, so we cannot acquire it
            return null;
        }
    }

    public async Task AddResourceAsync(ResourceEntity resource)
    {
        resource.PartitionKey = "ResourcePool"; // Ensure the partition key is set to the correct value
        await _tableClient.AddEntityAsync(resource);
    }

    public async Task RefillPoolAsync()
    {
        var availableResources = await GetAvailableResourcesAsync();
        var availableResourcesCount = availableResources.Count;
        if (availableResourcesCount < MinimumResources)
        {
            var refillCount = MinimumResources - availableResourcesCount + Buffer;

            // Logic to refill the pool, e.g., adding new resources
            // This is a placeholder; implement your own logic here
            for (int i = 0; i < refillCount; i++)
            {
                var newResource = new ResourceEntity();
                await AddResourceAsync(newResource);
            }
        }
    }

    // There is no ToListAsync for AsyncPageable<T> in Azure.Data.Tables.
    // You need to manually enumerate the results as shown in GetAllResourcesAsync.
    // If you want a reusable extension, you can add this:
    private static async Task<List<T>> ToListAsync<T>(AsyncPageable<T> source) where T : notnull
    {
        var list = new List<T>();
        await foreach (var item in source)
        {
            list.Add(item);
        }
        return list;
    }

    public async Task<ResourceEntity> AllocateRandomResourceAsync()
    {
        ResourceEntity? allocatedResource = null;

        while (allocatedResource == null)
        {
            var availableResources = await GetAvailableResourcesAsync();
            if (availableResources.Count == 0)
            {
                throw new InvalidOperationException("No available resources to allocate.");
            }

            // Randomly select an index from the available resources
            var randomIndex = new Random().Next(availableResources.Count);
            allocatedResource = await TryAcquireResourceAsync(availableResources[randomIndex].RowKey);
        }

        return allocatedResource;
    }

    public async Task DeleteResourceAsync(string rowKey)
    {
        try
        {
            await _tableClient.DeleteEntityAsync("ResourcePool", rowKey);
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
        }        
    }

    public async Task<object?> GetResourceAsync(string rowKey)
    {
        try
        {
            var resource = await _tableClient.GetEntityAsync<ResourceEntity>("ResourcePool", rowKey);
            return resource.Value;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return null; // Resource not found
        }
    }
}
