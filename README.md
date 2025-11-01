# DistributedLockManager

DistributedLockManager is a robust .NET library for distributed locking using Redis and the RedLock algorithm. It is designed for scalable applications that require distributed synchronization and safe resource access across multiple processes or services.

## Features

- Redis-based distributed locking (RedLock algorithm)
- Supports both void and result-returning operations
- Configurable lock expiry, wait, and retry times
- Cancellation support via `CancellationToken`
- Easy integration with dependency injection
- Thread-safe and automatic lock release

## Installation

Install via NuGet (coming soon):

```bash
dotnet add package Tricksfor.DistributedLockManager
```

Or build from source:

```bash
dotnet build DistributedLockManager.sln -c Release
```

## Prerequisites

- .NET 8.0 or later
- Redis server (local, docker or remote)

## Usage

### 1. Register Services

Add the required services to your dependency injection container:

```csharp
services.AddDistributedLockManager();
```

#### If you are using .NET Aspire

You do **not** need to manually register `IConnectionMultiplexer` if you have already added Redis to your Aspire project as described in the [Microsoft Aspire documentation](https://learn.microsoft.com/en-us/dotnet/aspire/).

#### If you are not using Aspire

Register the Redis connection multiplexer:

```csharp
services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect("localhost:6379"));
```

### 2. Use in Your Code (Integration Test Style)

You can use the distributed lock service for both void and result-returning operations. Here is an example similar to the integration tests:

### Result Returning (of type TResult) Operations

```csharp
// For operations that return a value
var distributedLockService = serviceProvider.GetRequiredService<IDistributedLockService<TResult>>();
var result = await distributedLockService.RunWithLockAsync(
    async () => await ProcessDataAsync(),
    CancellationToken.None
);
```

### Void Operations

```csharp
await distributedLockService.RunWithLockAsync(
    async () => await ProcessDataAsync(),
    "process-data-lock",
    CancellationToken.None
);
```

### Custom Lock Parameters

```csharp
await distributedLockService.RunWithLockAsync(
    async () => await LongRunningOperation(),
    "long-operation-lock",
    CancellationToken.None,
    expiryInSecond: 60,    // Lock expires after 60 seconds
    waitInSecond: 15,      // Wait up to 15 seconds to acquire lock
    retryInSecond: 2       // Retry every 2 seconds
);
```

## Lock Parameters

- `expiryInSecond`: Duration the lock is held (default: 30 seconds)
- `waitInSecond`: Max time to wait for lock (default: 10 seconds)
- `retryInSecond`: Time between retries (default: 1 second)

## Exception Handling

`InvalidOperationException` is thrown if:
- Lock acquisition fails after the wait time
- The resource is already locked

Example:

```csharp
try
{
    await _lockService.RunWithLockAsync(...);
}
catch (InvalidOperationException ex)
{
    // Handle lock acquisition failure
}
catch (Exception ex)
{
    // Handle other errors
}
```

## License

This project is licensed under the MIT License. See the LICENSE file for details.
