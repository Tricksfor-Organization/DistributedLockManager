using DistributedLockManager.Interfaces;
using DistributedLockManager.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RedLockNet;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using StackExchange.Redis;

namespace DistributedLockManager;

public static class CacheManagerConfiguration
{
    public static void AddDistributedLockManager(this IServiceCollection services)
    {
        services.AddSingleton<IDistributedLockFactory, RedLockFactory>(sp =>
        {
            var connectionMultiplexer = sp.GetRequiredService<IConnectionMultiplexer>();
            var loggerFactory = sp.GetRequiredService<ILoggerFactory>();

            return RedLockFactory.Create([new RedLockMultiplexer(connectionMultiplexer)], loggerFactory);
        });

        services.AddScoped<IDistributedLockService, DistributedLockService>();
        services.AddScoped(typeof(IDistributedLockService<>), typeof(DistributedLockService<>));
    }
}