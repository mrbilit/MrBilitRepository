using Microsoft.Extensions.DependencyInjection;

using MrBilit.Repository.Abstractions;
using MrBilit.Repository.Caching;
using MrBilit.Repository.Caching.CacheSynchronization;
using MrBilit.Repository.Caching.Initializer;
using MrBilit.Repository.Caching.MemoryCache;

namespace MrBilit.Repository.DI.Microsoft;

public static class DependencyInjectionConfiguration
{
    public static DependencyInjectionConfigurator AddMrBilitRepository(this IServiceCollection services)
    {
        services.AddScoped(typeof(ICacheProvider<>), typeof(CacheProvider<>));
        services.AddScoped<ICacheSynchronizationContext, CacheSynchronizationContext>();
        services.AddScoped<IServiceFactory, ServiceFactory>();
        services.AddScoped<ICachedRepositoryInitializer, CachedRepositoryInitializer>();
        return new DependencyInjectionConfigurator(services);
    }
}

public class DependencyInjectionConfigurator
{
    public IServiceCollection Services { get; }

    public DependencyInjectionConfigurator(IServiceCollection services)
    {
        Services = services;
    }

    public DependencyInjectionConfigurator AddMemoryCache()
    {
        Services.AddScoped(typeof(IListCacheProvider<>), typeof(MemoryListCacheProvider<>));
        Services.AddScoped(typeof(IMapCacheProvider<>), typeof(MemoryMapCacheProvider<>));
        Services.AddScoped<IListCacheProviderFactory, MemoryListCacheProviderFactory>();
        Services.AddScoped<IMapCacheProviderFactory, MemoryMapCacheProviderFactory>();
        return this;
    }
}
