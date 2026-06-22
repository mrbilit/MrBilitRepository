namespace MrBilit.Repository.Caching.CacheSynchronization
{
    public interface ICacheSynchronizationContext
    {
        public ValueTask BroadcastResyncRequest(Type type);
        public Task PerformResync(Type type);
    }
}
