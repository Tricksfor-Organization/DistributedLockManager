using DistributedLockManager.Interfaces;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using StackExchange.Redis;

namespace DistributedLockManager.Services;

public class DistributedLockService : IDistributedLockService
{
    private readonly RedLockFactory _redLockFactory;
    public DistributedLockService(IConnectionMultiplexer connectionMultiplexer)
    {
        var endPoints = connectionMultiplexer.GetEndPoints();
        List<RedLockEndPoint> redLockEndPoints = [.. endPoints];

        _redLockFactory = RedLockFactory.Create(redLockEndPoints);
    }

    public async Task RunWithLockAsync(Func<Task> func, string key, CancellationToken cancellationToken, int expiryInSecond = 30, int waitInSecond = 10, int retryInSecond = 1)
    {
        // blocks until acquired or 'wait' timeout
        await using var redLock = await _redLockFactory.CreateLockAsync(key, TimeSpan.FromSeconds(expiryInSecond), TimeSpan.FromSeconds(waitInSecond),
            TimeSpan.FromSeconds(retryInSecond), cancellationToken);

        if (redLock.IsAcquired)
            await func();
    }

    public async Task RunWithLockAsync(Task task, string key, CancellationToken cancellationToken, int expiryInSecond = 30, int waitInSecond = 10, int retryInSecond = 1)
    {
        // blocks until acquired or 'wait' timeout
        await using var redLock = await _redLockFactory.CreateLockAsync(key, TimeSpan.FromSeconds(expiryInSecond), TimeSpan.FromSeconds(waitInSecond),
            TimeSpan.FromSeconds(retryInSecond), cancellationToken);

        if (redLock.IsAcquired)
            await task;
    }
}

public class DistributedLockService<TResult> : IDistributedLockService<TResult>
{
    private readonly RedLockFactory _redLockFactory;
    public DistributedLockService(IConnectionMultiplexer connectionMultiplexer)
    {
        var endPoints = connectionMultiplexer.GetEndPoints();
        List<RedLockEndPoint> redLockEndPoints = [.. endPoints];

        _redLockFactory = RedLockFactory.Create(redLockEndPoints);
    }

    public async Task<TResult> RunWithLockAsync(Func<Task<TResult>> func, string key, CancellationToken cancellationToken, int expiryInSecond = 30, int waitInSecond = 10, int retryInSecond = 1)
    {
        // blocks until acquired or 'wait' timeout
        await using var redLock = await _redLockFactory.CreateLockAsync(key, 
            expiryTime: TimeSpan.FromSeconds(expiryInSecond), 
            waitTime: TimeSpan.FromSeconds(waitInSecond),
            retryTime: TimeSpan.FromSeconds(retryInSecond), cancellationToken);

        if (redLock.IsAcquired)
            return await func();
            
        // if we did not acquire the lock, throw an exception to let the client know that must try again!
        throw new InvalidOperationException("Resource is locked right now. Try again later!");
    }

    public async Task<TResult> RunWithLockAsync(Task<TResult> task, string key, CancellationToken cancellationToken, int expiryInSecond = 30, int waitInSecond = 10, int retryInSecond = 1)
    {
        // blocks until acquired or 'wait' timeout
        await using var redLock = await _redLockFactory.CreateLockAsync(key, TimeSpan.FromSeconds(expiryInSecond), TimeSpan.FromSeconds(waitInSecond),
            TimeSpan.FromSeconds(retryInSecond), cancellationToken);

        if (redLock.IsAcquired)
            return await task;

        // if we did not acquire the lock, throw an exception to let the client know that must try again!
        throw new InvalidOperationException("Resource is locked right now. Try again later!");
    }
}