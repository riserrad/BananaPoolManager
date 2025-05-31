using System;
using Azure;
using Azure.Data.Tables;

namespace BananaPoolLocker.Models;

public class ResourceEntity : ITableEntity
{
    public string PartitionKey { get; set; } = "ResourcePool";
    public string RowKey { get; set; } = Guid.NewGuid().ToString(); // Unique identifier for the resource
    public string Status { get; set; } = "Available"; // Available, InUse, or Provisioning

    public DateTimeOffset? Timestamp { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public ETag ETag { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
}
