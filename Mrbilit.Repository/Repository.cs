using Microsoft.EntityFrameworkCore;

using MrBilit.Repository.Abstractions;

namespace MrBilit.Repository;

public class Repository<T> : Ardalis.Specification.EntityFrameworkCore.RepositoryBase<T>, IRepository<T> where T : class
{
    public Repository(DbContext dbContext) : base(dbContext)
    {
    }
}
