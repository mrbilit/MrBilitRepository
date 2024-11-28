using System.Reflection;

using Microsoft.EntityFrameworkCore;

using Mrbilit.Repository;
using Mrbilit.Repository.Data;

using MrBilit.Repository;

using TypeExtensions = Mrbilit.Repository.Common.Extensions.TypeExtensions;

namespace Mrbilit.Persistence.Providers.SqlServer;

public class SqlServerUtility : IDatabaseUtility
{
    private static List<Type> _entities;
    private static List<Type> _views;
    private static Dictionary<Type, bool> _entityUsageStatus;
    private readonly ApplicationDbContextBase _applicationDbContext;
    public SqlServerUtility(ApplicationDbContextBase context)
    {
        _applicationDbContext = context;
    }
    public void Init(Assembly assembly)
    {
        _entities = new List<Type>();
        _views = new List<Type>();
        _entityUsageStatus = new Dictionary<Type, bool>();
        var allRepos = assembly.GetTypes().Where(t => TypeExtensions.IsSubclassOfRawGeneric(typeof(Repository<>), t));
        foreach (var repo in allRepos)
        {
            var genericArgument = repo.BaseType.GetGenericArguments()[0];
            if (IsModel(genericArgument))
            {
                if (IsDbView(genericArgument))
                {
                    _views.Add(genericArgument);
                }
                else
                {
                    _entities.Add(genericArgument);
                }
            }

        }
    }

    private bool IsDbView(Type t)
    {
        var mapping = _applicationDbContext.Model.FindEntityType(t);
        if (mapping == null)
            return false;
        var schema = mapping.GetSchema() ?? "dbo";
        var tableName = mapping.GetTableName();
        var result = _applicationDbContext.Database.ExecuteSqlRaw(
        $"SELECT CASE WHEN EXISTS (SELECT * FROM INFORMATION_SCHEMA.VIEWS WHERE TABLE_SCHEMA = '{schema}' AND TABLE_NAME = '{tableName}') THEN 1 ELSE 0 END");
        return result == 1;
    }

    private bool IsModel(Type type)
    {
        return _applicationDbContext.Model.FindEntityType(type) != null;
    }

    public bool IsUsedEntityTypes(Type type)
    {
        if (_entityUsageStatus.ContainsKey(type))
        {
            _entityUsageStatus.TryGetValue(type, out var result);
            return result;
        }
        else
        {
            var status = ContainsEntityType(type);
            _entityUsageStatus.Add(type, status);
            return status;
        }
    }

    private bool ContainsEntityType(Type type)
    {
        if (type.IsPrimitive || type == typeof(string) || type == typeof(decimal) || type == typeof(DateTime))
        {
            return false;
        }

        if (_entities.Any(p => p == type))
            return true;

        var nestedTypes = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
        .Select(f => f.FieldType)
        .Distinct();

        foreach (var nestedType in nestedTypes)
        {
            if (ContainsEntityType(nestedType))
                return true;
        }

        return false;
    }

    public bool IsView(Type type)
    {
        return _views.Any(p => p == type);
    }
}
