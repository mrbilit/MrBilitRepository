using Microsoft.Extensions.Logging;

namespace MrBilit.Repository.Caching.CacheSynchronization;

public class CacheSynchronizationContext : ICacheSynchronizationContext
{
    private readonly IEnumerable<ICacheSynchronizationPublisher> _publishers;
    private readonly IServiceFactory _serviceFactory;
    private readonly ILogger<CacheSynchronizationContext> _logger;

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

    public async ValueTask PerformResync(Type type)
    {
        if (!type.IsAssignableTo(typeof(ISynchronizable)))
        {
            _logger?.LogCritical("Invalid type name was specified for sync: " + type.Name + ". This type does not implement ISyncronizable interface");
            return;
        }

        var typeInstance = _serviceFactory.InstantiateByType<ISynchronizable>(type);
        if (typeInstance == null)
        {
            _logger?.LogCritical("Unable to instanciate type " + type.Name + " as ISynchronizable");
            return;
        }

        // Calling resync
        await typeInstance.Resync();
    }
}
