using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Mrbilit.Persistence.Providers.SqlServer;
using Mrbilit.Repository;
using Mrbilit.Repository.Data;
using Mrbilit.Repository.Initializer;

using MrBilit.Repository.Abstractions;
using MrBilit.Repository.Caching;
using MrBilit.Repository.Caching.CacheSynchronization;
using MrBilit.Repository.Caching.MemoryCache;

namespace MrBilit.Repository.DI.Microsoft;

public static class DependencyInjectionConfiguration
{
    public static DependencyInjectionConfigurator AddMrBilitRepository(this IServiceCollection services)
    {
        services.AddScoped(typeof(ICacheProvider<>), typeof(CacheProvider<>));
        services.AddScoped<ICacheSynchronizationContext, CacheSynchronizationContext>();
        services.AddScoped<IServiceFactory, ServiceFactory>();
        services.AddScoped<IRepositoryInitializer, RepositoryInitializer>();
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

    public DependencyInjectionConfigurator AddDbContext<T>(Action<DbContextOptionsBuilder>? optionsBuilder = null) where T : ApplicationDbContextBase
    {
        Services.AddDbContext<ApplicationDbContextBase, T>(optionsBuilder);
        return this;
    }

    public DependencyInjectionConfigurator AddDbContext<TDbContext, TReadOnlyDbContext>(Action<DbContextOptionsBuilder>? optionsBuilder = null, Action<DbContextOptionsBuilder>? readOnlyOptionsBuilder = null) where TDbContext : ApplicationDbContextBase where TReadOnlyDbContext : ApplicationDbContextBaseReadOnlyBase
    {
        Services.AddDbContext<ApplicationDbContextBase, TDbContext>(optionsBuilder);
        Services.AddDbContext<ApplicationDbContextBaseReadOnlyBase, TReadOnlyDbContext>(readOnlyOptionsBuilder);
        return this;
    }

    public DependencyInjectionConfigurator UseSqlServer()
    {
        Services.AddScoped<IDatabaseUtility, SqlServerUtility>();
        return this;
    }


}
