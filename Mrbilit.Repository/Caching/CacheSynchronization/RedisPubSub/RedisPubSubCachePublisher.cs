using Microsoft.Extensions.Options;

using StackExchange.Redis;

using System.Text.Json;

namespace MrBilit.Repository.Caching.CacheSynchronization.RedisPubSub;

public class RedisPubSubCachePublisher : ICacheSynchronizationPublisher
{
    private ConnectionMultiplexer _redis;
    private string _connectionString;
    private object _lock = new object();

    public RedisPubSubCachePublisher(IOptions<RedisPubSubOptions> options)
    {
        _connectionString = options.Value.ConnectionString;
    }

    public async ValueTask NotifyChanges<T>() where T : ISynchronizable
    {
        var synchronizableType = typeof(T);
        await NotifyChanges(synchronizableType);
    }

    public async ValueTask NotifyChanges(Type synchronizableType)
    {
        if (synchronizableType is null || synchronizableType.AssemblyQualifiedName == null)
        {
            throw new ArgumentNullException(nameof(synchronizableType));
        }

        if (!synchronizableType.IsAssignableTo(typeof(ISynchronizable)))
        {
            throw new ArgumentException("The provided type must be derived from ISynchronizable");
        }

        if (synchronizableType.IsAbstract || synchronizableType.IsGenericType)
        {
            throw new ArgumentException("The provided type must not be abstract or generic type");
        }

        await SendMessage(new RedisPubSubSyncMessage
        {
            TypeName = synchronizableType.AssemblyQualifiedName
        });
    }

    private async ValueTask SendMessage(RedisPubSubSyncMessage message)
    {
        lock (_lock)
        {
            if (_redis == default || !_redis.IsConnected)
                _redis = ConnectionMultiplexer.Connect(_connectionString);
        }
        var pubSub = _redis.GetSubscriber();
        await pubSub.PublishAsync(RedisPubSubCacheSubscriber.ChannelName, JsonSerializer.Serialize(message));
    }
}
