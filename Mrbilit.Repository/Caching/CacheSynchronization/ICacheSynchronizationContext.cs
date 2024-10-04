namespace MrBilit.Repository.Caching.CacheSynchronization
{
    public interface ICacheSynchronizationContext
    {
        public ValueTask BroadcastResyncRequest(Type type);
        public ValueTask PerformResync(Type type);
    }
}
