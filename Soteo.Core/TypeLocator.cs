using System.Collections.Immutable;
using System.Reflection;
using Soteo.Core.Interfaces;

namespace Soteo.Core;

public class TypeLocator : ITypeLocator
{
    public static readonly TypeLocator Empty = new([], []);
    
    public IReadOnlyList<Type> Types { get; }
    
    public TypeLocator(params IReadOnlyList<Type> types) : this([], types) { }
    
    public TypeLocator(params IReadOnlyList<Assembly> assemblies) : this(assemblies, []) { }
    
    public TypeLocator(IReadOnlyList<Assembly> assemblies, IReadOnlyList<Type> types)
    {
        Types = assemblies.SelectMany(it => it.ExportedTypes).Union(types).ToImmutableList();
    }
    
    public IReadOnlyList<Type> ConcreteSubclassesOf<T>(Func<Type, bool>? where = null)
    {
        where ??= _ => true;
        return Types
            .Where(it => !it.IsAbstract && it.IsAssignableTo(typeof(T)))
            .Where(where)
            .OrderBy(it => it.FullName)
            .ToImmutableList();
    }
    
    public IReadOnlyList<T> InstanceSubclassesOf<T>(Func<Type, bool>? where = null)
    {
        return ConcreteSubclassesOf<T>(where)
            .Select(it => (T)Activator.CreateInstance(it))
            .ToImmutableList();
    }
}
