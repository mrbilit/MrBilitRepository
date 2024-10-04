using MrBilit.Repository.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MrBilit.Repository.Caching.MemoryCache
{
    public class MemoryMapCacheProviderFactory : IMapCacheProviderFactory
    {
        public IMapCacheProvider<TValue> Create<TValue>(Func<TValue, string> keySelector) where TValue : class
        {
            return new MemoryMapCacheProvider<TValue>(keySelector);
        }
    }
}
