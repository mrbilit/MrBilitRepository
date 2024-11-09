using Microsoft.EntityFrameworkCore;

namespace Mrbilit.Repository.Data;

public abstract class ApplicationDbContextBase : DbContext
{
    public ApplicationDbContextBase(DbContextOptions<ApplicationDbContextBase> options) : base(options)
    {
    }
    protected ApplicationDbContextBase(DbContextOptions options) : base(options)
    {

    }
}
