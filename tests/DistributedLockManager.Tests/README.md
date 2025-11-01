Integration test project for DistributedLockManager
=================================================

This test project contains integration tests for `DistributedLockService` that run against a
real Redis instance started in a Docker container (using DotNet.Testcontainers). The tests
exercise the real locking behavior (acquire, hold, and mutual exclusion) and therefore are
integration tests rather than pure unit tests.

What the tests do
------------------
- Start a temporary Redis container (image: `redis:latest`) via Testcontainers.
- Create a `ConnectionMultiplexer` connected to that container and register it in DI.
- Call `services.AddDistributedLockManager()` (the production DI registration) and resolve
    `IDistributedLockService<bool>` from the scoped provider.
- Run two integration tests:
    - `RunWithLockAsync_acquires_and_executes_action` — verifies a simple action runs when the lock is acquired.
    - `RunWithLockAsync_prevents_concurrent_execution` — starts multiple concurrent tasks competing
        for the same lock and asserts that at most one task enters the critical section at a time.

Key points
----------
- These tests are integration tests (they use a real Redis instance). They do not require
    changing production code — instead they rely on the DI registration (`AddDistributedLockManager`) and
    registering the test `IConnectionMultiplexer` pointed at the test container.
- Because they start Docker containers, you must have Docker (or a compatible container runtime)
    available and running on the machine where the tests are executed.

How to run the tests
--------------------
From the repository root (requires Docker running):

```bash
dotnet restore
dotnet test tests/DistributedLockManager.Tests
```

Run a single test (example):

```bash
dotnet test tests/DistributedLockManager.Tests --filter "TestName~concurrent"
```

Troubleshooting
---------------
- If Docker is not running or available, the Testcontainers setup will fail and the tests will error.
- If the Redis container cannot start (port conflicts, permissions), check your Docker daemon logs
    and ensure your environment allows starting containers.

Notes for contributors
----------------------
- The current integration tests provide realistic validation of the locking behavior and can be
    used as examples for writing additional integration tests. If you'd like to add fast unit tests
    that avoid Docker, consider introducing a small abstraction around the lock/factory so tests can
    substitute a fake implementation — but this is optional and not required for the existing tests.

License and authorship: see project root README and LICENSE files.
