using Microsoft.Extensions.Logging;
using Mrbilit.Repository.Caching.CacheSynchronization;
using System.Collections.Concurrent;

namespace MrBilit.Repository.Caching.CacheSynchronization;

public class CacheSynchronizationContext : ICacheSynchronizationContext
{
    private readonly IEnumerable<ICacheSynchronizationPublisher> _publishers;
    private readonly IServiceFactory _serviceFactory;
    private readonly ILogger<CacheSynchronizationContext> _logger;
    private static readonly ConcurrentDictionary<Type, ResyncState> _states = new();

    public CacheSynchronizationContext(IEnumerable<ICacheSynchronizationPublisher> publishers, IServiceFactory serviceFactory, ILogger<CacheSynchronizationContext> logger)
    {
        _publishers = publishers;
        _serviceFactory = serviceFactory;
        _logger = logger;
    }

    public async ValueTask BroadcastResyncRequest(Type type)
    {
        foreach (var publisher in _publishers)
        {
            await publisher.NotifyChanges(type);
        }
    }

    public async Task PerformResync(Type type)
    {
        if (!type.IsAssignableTo(typeof(ISynchronizable)))
        {
            _logger?.LogCritical(
                "Invalid type specified for sync: {TypeName}",
                type.Name);

            return;
        }

        var typeInstance = _serviceFactory.InstantiateByType<ISynchronizable>(type);

        if (typeInstance == null)
        {
            _logger?.LogCritical(
                "Unable to instantiate type {TypeName} as ISynchronizable",
                type.Name);

            return;
        }

        var state = _states.GetOrAdd(type, _ => new ResyncState());

        // Reject if already scheduled
        if (Interlocked.CompareExchange(ref state.Scheduled, 1, 0) != 0)
            return;


        // Random delay: 1-20 seconds
        await Task.Delay(TimeSpan.FromSeconds(Random.Shared.Next(1, 21)));

        await state.Semaphore.WaitAsync();
        Volatile.Write(ref state.Scheduled, 0);


        try
        {
            await typeInstance.Resync();
        }
        finally
        {
            state.Semaphore.Release();
        }
    }
}
