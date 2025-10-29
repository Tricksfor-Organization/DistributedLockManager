namespace DistributedLockManager.Interfaces;

public interface IDistributedLockService
{
    Task RunWithLockAsync(Func<Task> func, string key, CancellationToken cancellationToken, int expiryInSecond = 30, int waitInSecond = 10, int retryInSecond = 1);
    Task RunWithLockAsync(Task task, string key, CancellationToken cancellationToken, int expiryInSecond = 30, int waitInSecond = 10, int retryInSecond = 1);
}

public interface IDistributedLockService<TResult>
{
    Task<TResult> RunWithLockAsync(Func<Task<TResult>> func, string key, CancellationToken cancellationToken, int expiryInSecond = 30, int waitInSecond = 10, int retryInSecond = 1);
    Task<TResult> RunWithLockAsync(Task<TResult> task, string key, CancellationToken cancellationToken, int expiryInSecond = 30, int waitInSecond = 10, int retryInSecond = 1);
}