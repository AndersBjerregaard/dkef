using System.Collections.Concurrent;
using System.Reflection;

using Dkef.Attributes;
using Dkef.Domain;

namespace Dkef.Configuration;

public sealed class SortablePropertyConfig
{
    private readonly ConcurrentDictionary<Type, HashSet<string>> _allowedSortableProperties = new();

    public SortablePropertyConfig(params Assembly[] assembliesToScan)
    {
        Initialize(assembliesToScan);
    }

    private void Initialize(params Assembly[] assembliesToScan)
    {
        foreach (var assembly in assembliesToScan)
        {
            ScanAssembly(assembly);
        }
    }

    private void ScanAssembly(Assembly assembly)
    {
        var entityTypes = assembly.GetTypes()
            .Where(x => x.IsSubclassOf(typeof(DomainClass)));

        foreach (var type in entityTypes)
        {
            var sortableProperties = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            ScanProperties(type, sortableProperties);

            // Id is sortable by default
            if (type.GetProperty("Id", BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase) != null)
            {
                sortableProperties.Add("Id");
            }

            if (sortableProperties.Any())
            {
                _allowedSortableProperties.TryAdd(type, sortableProperties);
            }
        }
    }

    private void ScanProperties(Type type, HashSet<string> collection)
    {
        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (prop.GetCustomAttribute<SortableAttribute>() is not null)
            {
                collection.Add(prop.Name);
            }
        }
    }

    public bool IsPropertySortable<TEntity>(string propertyName) => IsPropertySortable(typeof(TEntity), propertyName);

    public bool IsPropertySortable(Type entityType, string propertyName)
    {
        if (_allowedSortableProperties.TryGetValue(entityType, out var properties))
        {
            return properties.Contains(propertyName);
        }
        return false;
    }

    public HashSet<string> GetAllowedSortableProperties<TEntity>() => GetAllowedSortableProperties(typeof(TEntity));

    public HashSet<string> GetAllowedSortableProperties(Type entityType)
    {
        if (_allowedSortableProperties.TryGetValue(entityType, out var properties))
        {
            return properties;
        }
        return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    }
}