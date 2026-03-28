using System.Reflection;

namespace Soteo.Shared.Extensions;

public static class ReflectionExtensions
{
    extension(Type type)
    {
        public bool HasAttribute<T>() where T : Attribute =>
            CustomAttributeExtensions.GetCustomAttribute<T>(type) != null;
        
        public T? GetCustomAttribute<T>() where T : Attribute =>
            (T)type.GetCustomAttribute(typeof(T));
        
        public T GetRequiredAttribute<T>() where T : Attribute =>
            type.GetCustomAttribute<T>() ?? throw new ArgumentException($"{typeof(T)} not found on {type}.");
        
        public IEnumerable<Type> BaseTypes
        {
            get
            {
                foreach (var i in type.GetInterfaces())
                {
                    yield return i;
                }
                
                Type? currentBaseType = type.BaseType;
                while (currentBaseType != null)
                {
                    yield return currentBaseType;
                    currentBaseType = currentBaseType.BaseType;
                }
            }
        }
    }
}