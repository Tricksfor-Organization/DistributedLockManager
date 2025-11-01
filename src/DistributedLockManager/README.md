# DistributedLockManager

DistributedLockManager is a small .NET library that provides distributed locking using Redis. It helps coordinate access to shared resources across multiple processes or services.

## Install

Install from NuGet `Tricksfor.DistributedLockManager`.

```powershell
dotnet add package Tricksfor.DistributedLockManager
```

## Usage

Use the `IDistributedLockService` to run code under a distributed lock:

```csharp
await lockService.RunWithLockAsync(
    async () => { /* your code */ },
    "resource:123:operation",
    cancellationToken
);
```

See the GitHub repository for full docs and examples:

https://github.com/Tricksfor-Organization/DistributedLockManager

---

This README will be included in the NuGet package and displayed on nuget.org when the package is uploaded.
