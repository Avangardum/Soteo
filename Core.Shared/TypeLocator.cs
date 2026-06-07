using System.Collections.Immutable;
using System.Reflection;
using Soteo.Util;

namespace Soteo.Core.Shared;

public static class TypeLocator
{
    private static readonly LateInit<ImmutableList<Type>> LateInitTypes = new();
    
    public static void Init(params IReadOnlyList<Assembly> assemblies) =>
        LateInitTypes.Value = assemblies.SelectMany(it => it.ExportedTypes).ToImmutableList();

    public static ImmutableList<Type> Types => LateInitTypes;
    
    public static ImmutableList<Type> ConcreteSubclassesOf<T>(Func<Type, bool>? where = null) // todo return interface
    {
        where ??= _ => true;
        return Types
            .Where(it => !it.IsAbstract && it.IsAssignableTo(typeof(T)))
            .Where(where)
            .OrderBy(it => it.FullName)
            .ToImmutableList();
    }
    
    public static ImmutableList<T> InstanceSubclassesOf<T>(Func<Type, bool>? where = null)
    {
        return ConcreteSubclassesOf<T>(where)
            .Select(it => (T)Activator.CreateInstance(it))
            .ToImmutableList();
    }
}
