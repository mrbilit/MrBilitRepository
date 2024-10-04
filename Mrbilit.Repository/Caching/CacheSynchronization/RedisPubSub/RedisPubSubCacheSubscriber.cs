using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using StackExchange.Redis;

using System.Reflection;
using System.Text.Json;

namespace MrBilit.Repository.Caching.CacheSynchronization.RedisPubSub;

public class RedisPubSubCacheSubscriber : ICacheSynchronizationSubscriber
{
    private readonly string _connectionString;
    private readonly ILogger<RedisPubSubCacheSubscriber> _logger;
    private readonly IServiceFactory _serviceFactory;
    private ConnectionMultiplexer? _redis;

    public RedisPubSubCacheSubscriber(IOptions<RedisPubSubOptions> options, ILogger<RedisPubSubCacheSubscriber> logger, IServiceFactory serviceFactory)
    {
        _connectionString = options.Value.ConnectionString;
        _logger = logger;
        _serviceFactory = serviceFactory;
    }

    public async Task StartSubscription()
    {
        if (_redis == default || !_redis.IsConnected)
            _redis = await ConnectionMultiplexer.ConnectAsync(_connectionString);
        var pubsub = _redis.GetSubscriber();

        await pubsub.SubscribeAsync(ChannelName, async (channel, messageStr) =>
        {
            var message = JsonSerializer.Deserialize<RedisPubSubSyncMessage>(messageStr.ToString());

            // Instantiating and checking constraints
            if (message?.TypeName is null)
            {
                _logger?.LogCritical("Invalid messages received by redis sync subscriber: " + messageStr.ToString());
                return;
            }

            var type = Type.GetType(message.TypeName);
            if (type == null)
            {
                _logger?.LogCritical("Invalid type name was specified for sync: " + message.TypeName);
                return;
            }

            await _serviceFactory.InstantiateByTypeAndRunScoped<CacheSynchronizationContext>(async context => await context.PerformResync(type));
        });
    }

    public static string ChannelName => "CacheManager_" + Assembly.GetEntryAssembly().GetName().Name;
}
