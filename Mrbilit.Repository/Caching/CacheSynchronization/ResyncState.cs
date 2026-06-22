namespace Mrbilit.Repository.Caching.CacheSynchronization;

public sealed class ResyncState
{
    public int Scheduled;
    public SemaphoreSlim Semaphore { get; } = new(1, 1);
}
