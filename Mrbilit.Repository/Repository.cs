using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;

using Microsoft.EntityFrameworkCore;

using Mrbilit.Repository;
using Mrbilit.Repository.Data;

using MrBilit.Repository.Abstractions;

namespace MrBilit.Repository;

public class Repository<T> : Ardalis.Specification.EntityFrameworkCore.RepositoryBase<T>, IRepository<T> where T : class
{
    private readonly ApplicationDbContextBaseReadOnlyBase? _readOnlyContext;
    private readonly IDatabaseUtility? _databaseUtility;
    private readonly ApplicationDbContextBase _applicationDbContextBase;

    public Repository(ApplicationDbContextBase dbContext, ApplicationDbContextBaseReadOnlyBase? readonlyDbContext, IDatabaseUtility? databaseUtility) : base(dbContext)
    {
        _readOnlyContext = readonlyDbContext;
        _databaseUtility = databaseUtility;
        _applicationDbContextBase = dbContext;
    }

    public override async Task<bool> AnyAsync(ISpecification<T> specification, CancellationToken cancellationToken = default)
    {
        if (UseReadonlyDbContext(specification) == true)
        {
            return ApplySpecification(specification).Any();
        }
        else
        {
            return await base.AnyAsync(specification, cancellationToken);
        }
    }

    public override async IAsyncEnumerable<T> AsAsyncEnumerable(ISpecification<T> specification)
    {
        if (UseReadonlyDbContext(specification) == true)
        {
            foreach (var item in ApplySpecification(specification).AsEnumerable())
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

    public override async Task<T?> FirstOrDefaultAsync(ISpecification<T> specification, CancellationToken cancellationToken = default)
    {
        if (UseReadonlyDbContext(specification) == true)
        {
            return ApplySpecification(specification).FirstOrDefault();
        }
        else
        {
            return await base.FirstOrDefaultAsync(specification, cancellationToken);
        }
    }

    public override async Task<int> CountAsync(ISpecification<T> specification, CancellationToken cancellationToken = default)
    {
        if (UseReadonlyDbContext(specification) == true)
        {
            return ApplySpecification(specification).Count();
        }
        else
        {
            return await base.CountAsync(specification, cancellationToken);
        }
    }
    protected override IQueryable<T> ApplySpecification(ISpecification<T> specification, bool evaluateCriteriaOnly = false)
    {
        if (UseReadonlyDbContext(specification) == true)
        {
            return SpecificationEvaluator.Default.GetQuery(_readOnlyContext.Set<T>().AsQueryable(), specification, evaluateCriteriaOnly);
        }
        else
        {
            return base.ApplySpecification(specification, evaluateCriteriaOnly);
        }
    }

    protected override IQueryable<TResult> ApplySpecification<TResult>(ISpecification<T, TResult> specification)
    {
        if (UseReadonlyDbContext(specification) == true)
        {
            return SpecificationEvaluator.Default.GetQuery(_readOnlyContext.Set<T>().AsQueryable(), specification);
        }
        else
        {
            return base.ApplySpecification(specification);
        }
    }

    public override Task<TResult?> FirstOrDefaultAsync<TResult>(ISpecification<T, TResult> specification, CancellationToken cancellationToken = default) where TResult : default
    {
        if (UseReadonlyDbContext(specification) == true)
        {
            return ApplySpecification(specification).FirstOrDefaultAsync();
        }
        else
        {
            return base.FirstOrDefaultAsync(specification, cancellationToken);
        }
    }

    [Obsolete]
    public override Task<T?> GetBySpecAsync(ISpecification<T> specification, CancellationToken cancellationToken = default)
    {
        return FirstOrDefaultAsync(specification, cancellationToken);
    }
    [Obsolete]
    public override Task<TResult?> GetBySpecAsync<TResult>(ISpecification<T, TResult> specification, CancellationToken cancellationToken = default) where TResult : default
    {
        return FirstOrDefaultAsync(specification, cancellationToken);
    }

    public override async Task<List<T>> ListAsync(ISpecification<T> specification, CancellationToken cancellationToken = default)
    {
        if (UseReadonlyDbContext(specification) == true)
        {
            return ApplySpecification(specification).ToList();
        }
        else
        {
            return await base.ListAsync(specification, cancellationToken);
        }
    }

    public override async Task<List<TResult>> ListAsync<TResult>(ISpecification<T, TResult> specification, CancellationToken cancellationToken = default)
    {
        if (UseReadonlyDbContext(specification) == true)
        {
            return ApplySpecification(specification).ToList();
        }
        else
        {
            return await base.ListAsync(specification, cancellationToken);
        }
    }

    public override async Task<TResult?> SingleOrDefaultAsync<TResult>(ISingleResultSpecification<T, TResult> specification, CancellationToken cancellationToken = default) where TResult : default
    {
        if (UseReadonlyDbContext(specification) == true)
        {
            return ApplySpecification(specification).SingleOrDefault();
        }
        else
        {
            return await base.SingleOrDefaultAsync(specification, cancellationToken);
        }
    }

    public override async Task<T?> SingleOrDefaultAsync(ISingleResultSpecification<T> specification, CancellationToken cancellationToken = default)
    {
        if (UseReadonlyDbContext(specification) == true)
        {
            return ApplySpecification(specification).SingleOrDefault();
        }
        else
        {
            return await base.SingleOrDefaultAsync(specification, cancellationToken);
        }
    }


    private bool? UseReadonlyDbContext(ISpecification<T> specification)
    {
        if (_readOnlyContext == null)
        {
            return false;
        }
        if (specification.AsNoTracking)
        {
            return true;
        }
        if (_databaseUtility == null)
        {
            throw new Exception("IDatabaseUtility implementation type is not initialized.");
        }
        if (_databaseUtility.IsView(typeof(T)))
        {
            return true;
        }
        return null;
    }

    private bool UseReadonlyDbContext<TResult>(ISpecification<T, TResult> specification)
    {
        var useReadOnlyContext = UseReadonlyDbContext(specification as ISpecification<T>);
        if (useReadOnlyContext != null)
        {
            return useReadOnlyContext.Value;
        }
        if (_databaseUtility == null)
        {
            throw new Exception("IDatabaseUtility implementation type is not initialized.");
        }
        if (_databaseUtility.IsUsedEntityTypes(typeof(TResult)))
        {
            Console.WriteLine("contains nested entity");

            return false;
        }
        return true;

    }

    public virtual T? FirstOrDefault(ISpecification<T> specification)
    {
        var query = ApplySpecification(specification);
        return query.FirstOrDefault();
    }

    public virtual TResult? FirstOrDefault<TResult>(ISpecification<T, TResult> specification)
    {
        return ApplySpecification(specification).FirstOrDefault();
    }

    public virtual List<T> List()
    {
        return _applicationDbContextBase.Set<T>().ToList();
    }

    public virtual List<T> List(ISpecification<T> specification)
    {
        return ApplySpecification(specification).ToList();
    }
}
