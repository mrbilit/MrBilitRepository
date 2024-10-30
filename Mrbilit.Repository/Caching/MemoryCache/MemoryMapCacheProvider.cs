using MrBilit.Repository.Abstractions;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MrBilit.Repository.Caching.MemoryCache
{
    public class MemoryMapCacheProvider<T> : INonSynchronizedCacheProvider<T>, IMapCacheProvider<T> where T : class
    {
        private ConcurrentDictionary<string, T>? _cache;
        private Func<T, string> _keySelector;

        public MemoryMapCacheProvider(Func<T, string> keySelector)
        {
            _keySelector = keySelector ?? throw new ArgumentNullException(nameof(keySelector));
        }

        public ValueTask<T?> GetValueOrDefaultAsync(string key)
        {
            if (_cache == null)
            {
                throw new InvalidOperationException("Memory map cache is not intialized yet.");
            }
            return ValueTask.FromResult(_cache.GetValueOrDefault(key));
        }

        public ValueTask InitAsync(IEnumerable<T> values)
        {
            _cache ??= new ConcurrentDictionary<string, T>();
            _cache.Clear();
            foreach (var value in values)
            {
                var key = _keySelector(value);
                if ((!string.IsNullOrEmpty(key)))
                {
                    _cache.AddOrUpdate(key, _ => value, (_, _) => value);
                }
            }

            return ValueTask.CompletedTask;
        }

        public async ValueTask ResyncAsync(IEnumerable<T> values)
        {
            await InitAsync(values);
        }
    }
}
