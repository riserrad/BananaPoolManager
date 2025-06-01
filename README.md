# BananaPoolManager

BananaPoolManager is a fun personal project of a flexible framework for managing pools of pre-provisioned resources, enabling fast assignment and efficient utilization. The idea is to leverage this code to pool any type of resource, such as servers, virtual machines, databases, or custom tenant-based applications.

Currently, I am using Azure Tables as storage for this project.

## Features

- **Resource Pool Management:** Easily manage resources in the pool with the Console App.

## Upcoming Features

- **Distributed Hosts:** Scale with distributed pools by adding multiple hosts.
- **Extensible Architecture:** Improved design will support pluggable allocation and refilling strategies.
- **Automated Alerts:** Receive notifications when resources are low or abnormal activity is detected.

## Requirements

- [.NET SDK 9](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)
- [Azurite](https://learn.microsoft.com/en-us/azure/storage/common/storage-use-azurite?tabs=visual-studio-code%2Cblob-storage) for local Azure Storage emulation

## Getting Started

1. Clone the repository.
2. Check the Unit Tests in `BananaPoolManager.Tests` to understand current functionalities.
3. Run the Console Application to play with it. `dotnet run --project BananaPoolManager.ConsoleApp` from the project's root folder.

## Console Application

Here are the available commands so far:

```bash
  list - List all resources
  allocate [count] - Allocate resources (default is 1)
  delete <rowKey> - Delete a resource by rowKey
  refill - Refill the resource pool
  exit - Exit the application
```

## Contributing

Contributions are welcome! Please open issues or submit pull requests for improvements.

## License

This project is licensed under the MIT License.