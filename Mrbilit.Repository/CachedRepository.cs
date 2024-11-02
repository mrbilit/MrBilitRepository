using Ardalis.Specification;

using Microsoft.EntityFrameworkCore;

using Mrbilit.Repository.Specifications;

using MrBilit.Repository.Caching;
using MrBilit.Repository.Caching.CacheSynchronization;
using MrBilit.Repository.Caching.Include;

using SpecificationPOC.Specification.Base;

namespace MrBilit.Repository;

public abstract class CachedRepository<T> : Repository<T>, ISynchronizable, IInitializable where T : class
{
    private readonly ICacheProvider<T> _cacheProvider;
    private readonly ICacheSynchronizationContext _cacheSynchronizationContext;
    private bool _cacheConfigured = false;
    private static IncludeSpecification<T>? s_includeSpecification = null;
    private static BaseSpecification<T>? s_baseSpecification = null;
    private readonly DbContext _dbContext;

    public CachedRepository(DbContext dbContext, ICacheProvider<T> cacheProvider, ICacheSynchronizationContext cacheSynchronizationContext) : base(dbContext)
    {
        _cacheProvider = cacheProvider;
        _cacheSynchronizationContext = cacheSynchronizationContext;
        _dbContext = dbContext;
        ConfigureCache();
        _cacheConfigured = true;
    }

    public CachedRepository(DbContext dbContext, DbContext readOnlyContext, ICacheProvider<T> cacheProvider, ICacheSynchronizationContext cacheSynchronizationContext) : base(dbContext, readOnlyContext)
    {
        _cacheProvider = cacheProvider;
        _cacheSynchronizationContext = cacheSynchronizationContext;
        _dbContext = dbContext;
        ConfigureCache();
        _cacheConfigured = true;
    }

    public async Task InitAsync()
    {
        await _cacheProvider.InitAsync(await GetCacheValues());
    }

    private async ValueTask<IEnumerable<T>> GetCacheValues()
    {
        var dbSet = _dbContext.Set<T>().AsQueryable();
        if (s_baseSpecification != null)
        {
            dbSet = ApplySpecification(s_baseSpecification);
        }
        if (s_includeSpecification != null)
        {
            dbSet = ApplyIncludes(dbSet, s_includeSpecification.Includes);
        }

        return await dbSet.ToListAsync();
    }

    private IQueryable<T> ApplyIncludes(IQueryable<T> set, IEnumerable<string> includes)
    {
        foreach (var include in includes)
        {
            set = set.Include(include);
        }
        return set;
    }

    protected abstract void ConfigureCache();

    protected void ConfigureCacheDefaultSpecification(IncludeSpecification<T> includeSpecification, BaseSpecification<T> specification = null)
    {
        s_includeSpecification = includeSpecification;
        s_baseSpecification = specification;
    }

    protected void EnableListCache()
    {
        if (_cacheConfigured)
            throw new InvalidOperationException("Cannot reconfigure cache after initialization");

        _cacheProvider.EnableListCache();
    }

    protected void AddMapCache(string name, Func<T, string> keySelector)
    {
        if (_cacheConfigured)
            throw new InvalidOperationException("Cannot reconfigure cache after initialization");

        _cacheProvider.AddMapCache(name, keySelector);
    }

    /// <inheritdoc/>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var changeCount = await base.SaveChangesAsync(cancellationToken);
        if (changeCount > 0)
        {
            await _cacheSynchronizationContext.BroadcastResyncRequest(GetType());
        }
        return changeCount;
    }

    public async override Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        var result = await base.AddAsync(entity, cancellationToken);
        await _cacheSynchronizationContext.BroadcastResyncRequest(GetType());
        return result;
    }

    public override async Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        var result = await base.AddRangeAsync(entities, cancellationToken);
        await _cacheSynchronizationContext.BroadcastResyncRequest(GetType());
        return result;
    }

    public override async Task DeleteAsync(T entity, CancellationToken cancellationToken = default)
    {
        await base.DeleteAsync(entity, cancellationToken);
        await _cacheSynchronizationContext.BroadcastResyncRequest(GetType());

    }

    public override async Task DeleteRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        await base.DeleteRangeAsync(entities, cancellationToken);
        await _cacheSynchronizationContext.BroadcastResyncRequest(GetType());
    }

    public override async Task UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        await base.UpdateAsync(entity, cancellationToken);
        await _cacheSynchronizationContext.BroadcastResyncRequest(GetType());
    }

    public override async Task UpdateRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default)
    {
        await base.UpdateRangeAsync(entities, cancellationToken);
        await _cacheSynchronizationContext.BroadcastResyncRequest(GetType());
    }

    /// <inheritdoc/>
    [Obsolete("Use FirstOrDefaultAsync<T> or SingleOrDefaultAsync<T> instead. The SingleOrDefaultAsync<T> can be applied only to SingleResultSpecification<T> specifications.")]
    public override Task<T?> GetBySpecAsync(ISpecification<T> specification, CancellationToken cancellationToken = default)
    {
        return FirstOrDefaultAsync(specification, cancellationToken);
    }

    /// <inheritdoc/>
    [Obsolete("Use FirstOrDefaultAsync<T> or SingleOrDefaultAsync<T> instead. The SingleOrDefaultAsync<T> can be applied only to SingleResultSpecification<T> specifications.")]
    public override Task<TResult> GetBySpecAsync<TResult>(ISpecification<T, TResult> specification, CancellationToken cancellationToken = default)
    {
        return FirstOrDefaultAsync<TResult>(specification, cancellationToken);
    }

    /// <inheritdoc/>
    public override async Task<T?> FirstOrDefaultAsync(ISpecification<T> specification, CancellationToken cancellationToken = default)
    {
        if (_cacheProvider.ListCacheEnabled)
        {
            return (await GetCacheList(specification)).FirstOrDefault();
        }
        else
        {
            return await base.FirstOrDefaultAsync(specification, cancellationToken);
        }
    }

    /// <inheritdoc/>
    public virtual async Task<TResult?> FirstOrDefaultAsync<TResult>(ISpecification<T, TResult> specification, CancellationToken cancellationToken = default)
    {
        if (_cacheProvider.ListCacheEnabled)
        {
            return (specification.Evaluate(await _cacheProvider.GetListAsync())).FirstOrDefault();
        }
        else
        {
            return await base.FirstOrDefaultAsync(specification, cancellationToken);
        }
    }

    /// <inheritdoc/>
    public override async Task<T?> SingleOrDefaultAsync(ISingleResultSpecification<T> specification, CancellationToken cancellationToken = default)
    {
        if (_cacheProvider.ListCacheEnabled)
        {
            return specification.Evaluate(await _cacheProvider.GetListAsync()).SingleOrDefault();
        }
        else
        {
            return await base.SingleOrDefaultAsync(specification, cancellationToken);
        }
    }

    /// <inheritdoc/>
    public override async Task<TResult> SingleOrDefaultAsync<TResult>(ISingleResultSpecification<T, TResult> specification, CancellationToken cancellationToken = default)
    {
        if (_cacheProvider.ListCacheEnabled)
        {
            return (await GetCacheList(specification)).SingleOrDefault();
        }
        else
        {
            return await base.SingleOrDefaultAsync(specification, cancellationToken);
        }
    }

    /// <inheritdoc/>
    public override async Task<List<T>> ListAsync(CancellationToken cancellationToken = default)
    {
        if (_cacheProvider.ListCacheEnabled)
        {
            return (await _cacheProvider.GetListAsync()).ToList();
        }
        else
        {
            return await base.ListAsync(cancellationToken);
        }
    }

    /// <inheritdoc/>
    public override async Task<List<T>> ListAsync(ISpecification<T> specification, CancellationToken cancellationToken = default)
    {
        if (_cacheProvider.ListCacheEnabled)
        {
            return (await GetCacheList(specification)).ToList();
        }
        else
        {
            return await base.ListAsync(specification, cancellationToken);
        }
    }

    /// <inheritdoc/>
    public override async Task<List<TResult>> ListAsync<TResult>(ISpecification<T, TResult> specification, CancellationToken cancellationToken = default)
    {
        if (_cacheProvider.ListCacheEnabled)
        {
            return (await GetCacheList(specification)).ToList();
        }
        else
        {
            return await base.ListAsync(specification, cancellationToken);
        }
    }

    /// <inheritdoc/>
    public override async Task<int> CountAsync(ISpecification<T> specification, CancellationToken cancellationToken = default)
    {
        if (_cacheProvider.ListCacheEnabled)
        {
            return (await GetCacheList(specification)).Count();
        }
        else
        {
            return await base.CountAsync(specification, cancellationToken);
        }
    }

    /// <inheritdoc/>
    public override async Task<int> CountAsync(CancellationToken cancellationToken = default)
    {
        if (_cacheProvider.ListCacheEnabled)
        {
            return (await _cacheProvider.GetListAsync()).Count();
        }
        else
        {
            return await base.CountAsync(cancellationToken);
        }
    }

    /// <inheritdoc/>
    public override async Task<bool> AnyAsync(ISpecification<T> specification, CancellationToken cancellationToken = default)
    {
        if (_cacheProvider.ListCacheEnabled)
        {
            return (await GetCacheList(specification)).Any();
        }
        else
        {
            return await base.AnyAsync(specification, cancellationToken);
        }
    }

    /// <inheritdoc/>
    public override async Task<bool> AnyAsync(CancellationToken cancellationToken = default)
    {
        if (_cacheProvider.ListCacheEnabled)
        {
            return (await _cacheProvider.GetListAsync()).Any();
        }
        else
        {
            return await base.AnyAsync(cancellationToken);
        }
    }

    /// <inheritdoc/>
    public override async IAsyncEnumerable<T> AsAsyncEnumerable(ISpecification<T> specification)
    {
        if (_cacheProvider.ListCacheEnabled)
        {
            foreach (var item in await GetCacheList(specification))
            {
                yield return item;
            }
        }
        else
        {
            await foreach (var item in base.AsAsyncEnumerable(specification))
            {
                yield return item;
            }
        }
    }

    public async Task<T?> GetByIdAsync<TId>(string mapNAme, TId id, CancellationToken cancellationToken = default)
    {
        if (id is null)
        {
            throw new Exception("Id is null");
        }
        var x = await _cacheProvider.GetByKeyAsync(mapNAme, id.ToString());
        return x;
    }

    public async Task Resync()
    {

        await _cacheProvider.ResyncAsync(await GetCacheValues());
    }

    private async ValueTask<IEnumerable<T>> GetCacheList(ISpecification<T> specification)
    {
        VerifyIncludes(specification);
        return specification.Evaluate(await _cacheProvider.GetListAsync());

    }

    private async ValueTask<IEnumerable<TResult>> GetCacheList<TResult>(ISpecification<T, TResult> specification)
    {
        VerifyIncludes(specification);
        return specification.Evaluate(await _cacheProvider.GetListAsync());

    }

    private static void VerifyIncludes(ISpecification<T> specification)
    {
        if (s_includeSpecification != null)
        {
            var includes = specification.IncludeStrings;
            if (includes == null || !includes.Any())
            {
                includes = specification.ConvertToStringIncludes();

            }
            if (!s_includeSpecification.ContainIncludes(includes))
            {
                throw new Exception("The requested includes in the specification are not correct.");
            }
        }
    }
}
