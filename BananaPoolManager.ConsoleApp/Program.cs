using BananaPoolManager.Models;
using BananaPoolManager.Services;

const string ConnectionString = "UseDevelopmentStorage=true";
const string TableName = "ResourcePoolConsoleTable";

var resourcePoolService = new ResourcePoolService(ConnectionString, TableName);
bool running = true;

Console.WriteLine("##############################################");
Console.WriteLine("### Welcome to the Banana Pool Manager App ###");
Console.WriteLine("##############################################");
Console.WriteLine("");
// Console.WriteLine("Commands: list | allocate [count] | delete <rowKey> | refill | exit");
HelpCommand();

while (running)
{
    Console.Write("> ");
    var input = Console.ReadLine()?.Trim().ToLowerInvariant();
    var parts = input?.Split(' ', StringSplitOptions.RemoveEmptyEntries);

    if (parts == null || parts.Length == 0) continue;

    switch (parts[0])
    {
        case "list":
            await ListResourcesCommandAsync(resourcePoolService);
            break;
        case "allocate":
            await AllocateResourcesCommandAsync(resourcePoolService, parts);
            break;
        case "delete":
            await DeleteResourcesCommandAsync(resourcePoolService, parts);
            break;
        case "refill":
            await RefillCommandAsync(resourcePoolService);
            break;
        case "exit":
            running = false;
            break;
        case "help":
            HelpCommand();
            break;
        default:
            Console.WriteLine("Unknown command. Please try again.");
            break;
    }
}

async Task DeleteResourcesCommandAsync(ResourcePoolService resourcePoolService, string[] parts)
{
    if (parts.Length < 2)
    {
        Console.WriteLine("Please provide a rowKey to delete.");
        return;
    }

    var rowKey = parts[1];
    try
    {
        await resourcePoolService.DeleteResourceAsync(rowKey);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error deleting resource: {ex.Message}");
        return;
    }
}

static async Task ListResourcesCommandAsync(ResourcePoolService resourcePoolService)
{
    var resources = await resourcePoolService.GetAllResourcesAsync();
    var availableResources = resources.Where(r => r.Status == ResourceEntityState.Available).ToList();
    var inUseResources = resources.Where(r => r.Status == ResourceEntityState.InUse).ToList();
    Console.WriteLine($"Total Resources: {resources.Count}");
    Console.WriteLine("---------------------");
    Console.WriteLine($"Resources in Use ({inUseResources.Count}):");

    foreach (var resource in inUseResources)
    {
        Console.WriteLine($"- {resource.RowKey} | Status: {resource.Status}");
    }

    Console.WriteLine("---------------------");
    Console.WriteLine($"Available Resources ({availableResources.Count}):");

    foreach (var resource in availableResources)
    {
        Console.WriteLine($"- {resource.RowKey} | Status: {resource.Status}");
    }
}

static async Task AllocateResourcesCommandAsync(ResourcePoolService resourcePoolService, string[] parts)
{
    var resourcesToAllocate = parts.Length > 1 ? int.Parse(parts[1]) : 1;

    List<Task<ResourceEntity>> allocationTasks = new List<Task<ResourceEntity>>();

    for (int i = 0; i < resourcesToAllocate; i++)
    {
        allocationTasks.Add(resourcePoolService.AllocateRandomResourceAsync());
    }

    var allocatedResources = await Task.WhenAll(allocationTasks);

    if (allocatedResources.Length == 0)
    {
        Console.WriteLine("No resources were allocated.");
        return;
    }
    else if (allocatedResources.Length < resourcesToAllocate)
    {
        Console.WriteLine($"Only {allocatedResources.Length} resources were allocated out of {resourcesToAllocate} requested.");
    }

    foreach (var resource in allocatedResources)
    {
        Console.WriteLine($"Allocated Resource: {resource.RowKey} | Status: {resource.Status}");
    }
}

static async Task RefillCommandAsync(ResourcePoolService resourcePoolService)
{
    try
    {
        var allResources = await resourcePoolService.GetAllResourcesAsync();
        var resourceCount = allResources.Count;

        Console.WriteLine($"Current resource count: {resourceCount}");

        await resourcePoolService.RefillPoolAsync();
        allResources = await resourcePoolService.GetAllResourcesAsync();
        resourceCount = allResources.Count;
        Console.WriteLine($"Resource pool refilled. New resource count: {resourceCount}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error refilling resource pool: {ex.Message}");
    }
}

static void HelpCommand()
{
    Console.WriteLine("Available commands:");
    Console.WriteLine("  list - List all resources");
    Console.WriteLine("  allocate [count] - Allocate resources (default is 1)");
    Console.WriteLine("  delete <rowKey> - Delete a resource by rowKey");
    Console.WriteLine("  refill - Refill the resource pool");
    Console.WriteLine("  exit - Exit the application");
}