using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using MrBilit.Repository.Caching.CacheSynchronization;

namespace Mrbilit.Repository.Caching.CacheSynchronization.RedisPubSub;

public class RedisConsumerBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;

    public RedisConsumerBackgroundService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            try
            {
                await scope.ServiceProvider.GetRequiredService<ICacheSynchronizationSubscriber>().StartSubscription();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }

            await Task.Delay(Timeout.Infinite, stoppingToken);

        }
    }
}
