using System;
using Azure;
using Azure.Data.Tables;

namespace BananaPoolLocker.Models;

public class ResourceEntity : ITableEntity
{
    public string PartitionKey { get; set; } = "ResourcePool";
    public string RowKey { get; set; } = Guid.NewGuid().ToString(); // Unique identifier for the resource
    public ResourceEntityState Status { get; set; } = ResourceEntityState.Available; // Available, InUse, or Provisioning

    public DateTimeOffset? Timestamp { get; set; } = DateTimeOffset.UtcNow;
    public ETag ETag { get; set; }
}
