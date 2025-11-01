using System;
using System.Threading;
using System.Threading.Tasks;
using DistributedLockManager.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using NUnit.Framework;
using System.Linq;

namespace DistributedLockManager.Tests.Integration;

[TestFixture]
public class DistributedLockServiceIntegrationTests
{
    private IServiceScope? scope;
    private TestcontainersContainer? _redisContainer;
    private const string RedisImage = "redis:latest";
    private const int RedisPort = 6379;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _redisContainer = new TestcontainersBuilder<TestcontainersContainer>()
            .WithImage(RedisImage)
            .WithCleanUp(true)
            .WithName($"dtm-redis-{Guid.NewGuid():N}")
            .WithPortBinding(RedisPort, true)
            .Build();

        await _redisContainer.StartAsync();

        if (_redisContainer == null) Assert.Fail("Redis container not started");
        var container = _redisContainer!;
        var host = container.Hostname;
        var port = container.GetMappedPublicPort(RedisPort);
        var endpoint = $"{host}:{port}";

        // create the connection multiplexer and register it in DI
        using var mux = await ConnectionMultiplexer.ConnectAsync(endpoint);

        var services = new ServiceCollection();
        services.AddDistributedLockManager();
        services.AddSingleton<IConnectionMultiplexer>(mux);

        var provider = services.BuildServiceProvider();

        scope = provider.CreateScope();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        if (_redisContainer is not null)
            await _redisContainer.StopAsync();
    }

    [Test, Order(1)]
    public async Task RunWithLockAsync_acquires_and_executes_action()
    {
        if (scope == null)
        {
            Assert.Fail("Service scope not initialized");
            return;
        }

        var service = scope.ServiceProvider.GetRequiredService<IDistributedLockService<bool>>();

        var executed = false;
        executed = await service.RunWithLockAsync(async () => await SampleAsync(executed), "integration:test:key", CancellationToken.None, expiryInSecond: 5, waitInSecond: 2, retryInSecond: 1);

        Assert.IsTrue(executed, "Action should have been executed when lock acquired");
    }
    
    [Test, Order(2)]
    public async Task RunWithLockAsync_prevents_concurrent_execution()
    {
        if (scope == null)
        {
            Assert.Fail("Service scope not initialized");
            return;
        }

        var service = scope.ServiceProvider.GetRequiredService<IDistributedLockService<bool>>();
        var key = "integration:test:concurrent";

        int concurrentExecutions = 0;
        int maxConcurrent = 0;

        async Task<bool> CriticalSection()
        {
            Interlocked.Increment(ref concurrentExecutions);
            maxConcurrent = Math.Max(maxConcurrent, concurrentExecutions);
            // Simulate work
            await Task.Delay(500);
            Interlocked.Decrement(ref concurrentExecutions);
            return true;
        }

        // Start multiple tasks that will compete for the same lock
        var tasks = new Task<bool>[5];
        for (int i = 0; i < tasks.Length; i++)
        {
            tasks[i] = service.RunWithLockAsync(CriticalSection, key, CancellationToken.None, expiryInSecond: 5, waitInSecond: 5, retryInSecond: 1);
        }

        await Task.WhenAll(tasks);

        // Only one should ever be in the critical section at a time
        Assert.LessOrEqual(maxConcurrent, 1, $"Expected no concurrent executions, but saw {maxConcurrent}");
        Assert.IsTrue(tasks.All(t => t.Result), "All tasks should complete successfully");
    }

    private static async Task<bool> SampleAsync(bool executed)
    {
        executed = true;
        return await Task.FromResult(executed);
    }
}
