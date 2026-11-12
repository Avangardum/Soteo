namespace Soteo.Core;

public interface ITypeLocator
{
    IReadOnlyList<Type> Types { get; }
    IReadOnlyList<Type> ConcreteSubclassesOf<T>(Func<Type, bool>? where = null);
    IReadOnlyList<T> InstanceSubclassesOf<T>(Func<Type, bool>? where = null);
}
