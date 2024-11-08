using Microsoft.EntityFrameworkCore;

namespace Mrbilit.Repository.Data;

public abstract class ApplicationDbContextBaseReadOnlyBase : DbContext
{
    public ApplicationDbContextBaseReadOnlyBase(DbContextOptions<ApplicationDbContextBaseReadOnlyBase> options) : base(options)
    {
    }
    protected ApplicationDbContextBaseReadOnlyBase(DbContextOptions options) : base(options)
    {

    }
}
