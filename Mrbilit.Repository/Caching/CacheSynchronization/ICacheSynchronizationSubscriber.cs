namespace MrBilit.Repository.Caching.CacheSynchronization;

public interface ICacheSynchronizationSubscriber
{
    public Task StartSubscription();
}
