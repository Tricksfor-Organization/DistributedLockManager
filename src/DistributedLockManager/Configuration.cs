using DistributedLockManager.Interfaces;
using DistributedLockManager.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DistributedLockManager;

public static class CacheManagerConfiguration
{
    public static void AddDistributedLockManager(this IServiceCollection services)
    {
        services.AddScoped<IDistributedLockService, DistributedLockService>();
        services.AddScoped(typeof(IDistributedLockService<>), typeof(DistributedLockService<>));
    }
}