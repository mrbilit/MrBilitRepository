namespace MrBilit.Repository.Caching.CacheSynchronization;

public interface ICacheSynchronizationPublisher
{
    public ValueTask NotifyChanges<T>() where T : ISynchronizable;
    public ValueTask NotifyChanges(Type synchronizableType);
}
