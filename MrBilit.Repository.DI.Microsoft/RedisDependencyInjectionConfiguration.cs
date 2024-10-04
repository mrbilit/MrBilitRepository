using Microsoft.Extensions.DependencyInjection;

using MrBilit.Repository.Caching.CacheSynchronization;
using MrBilit.Repository.Caching.CacheSynchronization.RedisPubSub;

namespace MrBilit.Repository.DI.Microsoft;

public static class RedisDependencyInjectionConfiguration
{
    public static void AddRedisPubSubCacheSynchronizer(this DependencyInjectionConfigurator configurator, Action<RedisPubSubOptions> options)
    {
        configurator.Services.AddOptions<RedisPubSubOptions>().Configure(options);
        configurator.Services.AddSingleton<ICacheSynchronizationPublisher, RedisPubSubCachePublisher>();
        configurator.Services.AddSingleton<ICacheSynchronizationSubscriber, RedisPubSubCacheSubscriber>();
    }
}
