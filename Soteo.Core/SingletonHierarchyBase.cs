namespace Soteo.Core;

/// <summary>
/// Base class for a hierarchy of singleton classes like Ability, Status, Item. Ensures that each concrete derived type
/// has a single instance. Provides the static Instance method to get it and enforces its use instead of new.
/// </summary>
/// <remarks>
/// This design is used in order to detach state from logic and constants. This way it's possible to use singletons
/// for things like viewing description and stats without having to initialize all state and dependencies needed
/// to fully use them. If a singleton method needs to access state or dependencies, they are passed to that method
/// directly, thus allowing other methods to be used without them.
/// </remarks>
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
