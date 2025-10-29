# DistributedLockManager

A robust .NET library that provides distributed locking capabilities using Redis, designed for scalable applications requiring distributed synchronization.

## Features

- Redis-based distributed locking using the RedLock algorithm
- Support for both void and result-returning operations
- Configurable lock expiry, wait, and retry times
- Cancellation support via CancellationToken
- Easy integration with dependency injection
- Thread-safe operations
- Automatic lock release through IAsyncDisposable

## Installation

```bash
dotnet add package DistributedLockManager  # Coming soon to NuGet
```

## Quick Start

1. Add the required services to your dependency injection container:

```csharp
services.AddDistributedLockManager();
```

2. Inject and use the service:

```csharp
public class UserService
{
    private readonly IDistributedLockService _lockService;

    public UserService(IDistributedLockService lockService)
    {
        _lockService = lockService;
    }

    public async Task UpdateUserProfileAsync(int userId, UserProfile profile)
    {
        await _lockService.RunWithLockAsync(
            async () =>
            {
                // Your update logic here
                await SaveUserProfile(userId, profile);
            },
            key: $"user:{userId}:profile",
            cancellationToken: CancellationToken.None
        );
    }
}
```

## Usage Examples

### Basic Usage (void operations)

```csharp
// For operations that don't return a value
await _lockService.RunWithLockAsync(
    async () => await ProcessData(),
    "process-data-lock",
    CancellationToken.None
);
```

### With Return Value

```csharp
// For operations that return a value
var result = await _lockService<int>.RunWithLockAsync(
    async () => await CalculateTotal(),
    "calculate-total-lock",
    CancellationToken.None
);
```

### Custom Lock Parameters

```csharp
await _lockService.RunWithLockAsync(
    async () => await LongRunningOperation(),
    "long-operation-lock",
    CancellationToken.None,
    expiryInSecond: 60,    // Lock expires after 60 seconds
    waitInSecond: 15,      // Wait up to 15 seconds to acquire lock
    retryInSecond: 2       // Retry every 2 seconds
);
```

## Lock Parameters

- `expiryInSecond`: Duration the lock should be held (default: 30 seconds)
- `waitInSecond`: Maximum time to wait for lock acquisition (default: 10 seconds)
- `retryInSecond`: Time between retry attempts (default: 1 second)

## Exception Handling

The service throws `InvalidOperationException` when:
- Lock acquisition fails after the wait time
- The resource is already locked (for operations with return values)

Always implement appropriate error handling:

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

## Building from Source

```bash
dotnet build DistributedLockManager.sln -c Release
```

## Prerequisites

- .NET 8.0 or later
- Redis server
- StackExchange.Redis
- RedLock.net

## Contributing

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Roadmap

- [ ] Add unit tests
- [ ] Add integration tests with Redis
- [ ] Implement retry policies
- [ ] Add monitoring and metrics
- [ ] Create NuGet package
- [ ] Add CI/CD pipeline
- [ ] Add documentation site
