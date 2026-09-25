namespace Soteo.Core;

/// <summary>
/// Base class for a hierarchy of singleton classes like Ability, Status, Item. Ensures that each concrete derived type
/// has a single instance. Provides the static Instance method to get it and enforces its use instead of new.
/// </summary>
// TODO document the reasoning behind this design
public abstract class SingletonHierarchyBase<TBase> where TBase : class
{
    private static readonly Dictionary<Type, TBase> Instances = [];

    private static Type? _currentlyConstructedType;

    public static T Instance<T>() where T : TBase, new() => (T)Instance(typeof(T));

    public static TBase Instance(Type type)
    {
        if (!type.IsAssignableTo(typeof(TBase)))
            throw new ArgumentException($"{type} is not {typeof(TBase)}");

        if (Instances.TryGetValue(type, out TBase? existingInstance))
            return existingInstance;

        _currentlyConstructedType = type;
        var newInstance = (TBase)Activator.CreateInstance(type);
        _currentlyConstructedType = null;
        Instances[type] = newInstance;
        return newInstance;
    }

    protected SingletonHierarchyBase()
    {
        if (GetType() != _currentlyConstructedType)
        {
            throw new InvalidOperationException
            (
                $"Instances of {typeof(TBase)} should not be created with new, use the static Instance method instead"
            );
        }
    }
}
