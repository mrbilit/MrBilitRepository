using System.Reflection;


namespace Mrbilit.Repository;

public interface IDatabaseUtility
{
    public bool IsView(Type type);
    public bool IsUsedEntityTypes(Type type);
    public void Init(Assembly assembly);
}
