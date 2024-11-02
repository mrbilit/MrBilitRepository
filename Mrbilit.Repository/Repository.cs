using System.ComponentModel.Design;
using System.Reflection;

using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;

using Microsoft.EntityFrameworkCore;

using MrBilit.Repository.Abstractions;

namespace MrBilit.Repository;

public class Repository<T> : Ardalis.Specification.EntityFrameworkCore.RepositoryBase<T>, IRepository<T> where T : class
{
    private readonly DbContext _readOnlyContext;
    public Repository(DbContext dbContext) : base(dbContext)
    {
    }

    public Repository(DbContext dbContext, DbContext readonlyDbContext) : base(dbContext)
    {
        _readOnlyContext = dbContext;
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
            Console.WriteLine("readonlyDb is null");
            return false;
        }
        if (specification.AsNoTracking)
        {
            Console.WriteLine("query is noTracking");
            return true;
        }
        if (IsView())
        {
            Console.WriteLine("it is view");
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
        if (ContainsEntityType(typeof(TResult)))
        {
            Console.WriteLine("contains nested entity");

            return false;
        }
        return true;

    }

    private bool IsView()
    {
        var mapping = _readOnlyContext.Model.FindEntityType(typeof(T));
        if (mapping == null)
            return false;
        var schema = mapping.GetSchema;
        var tableName = mapping.GetTableName;
        var result = _readOnlyContext.Database.ExecuteSqlRaw(
        $"SELECT CASE WHEN EXISTS (SELECT * FROM INFORMATION_SCHEMA.VIEWS WHERE TABLE_SCHEMA = '{schema}' AND TABLE_NAME = '{tableName}') THEN 1 ELSE 0 END");
        return result == 1;
    }

    private bool IsEntity(Type type)
    {
        return _readOnlyContext.Model.FindEntityType(type) != null;
    }

    private bool ContainsEntityType(Type type)
    {
        if (IsEntity(type))
            return true;

        var nestedTypes = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
        .Select(f => f.FieldType)
        .Distinct();

        foreach (var nestedType in nestedTypes)
        {
            if (IsEntity(nestedType))
                return true;
        }

        return false;
    }
}
