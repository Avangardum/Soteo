using System.Reflection;

namespace Soteo.Util.Extensions;

public static class ReflectionExtensions
{
    extension (MemberInfo self)
    {
        public bool HasAttribute<T>() where T : Attribute => self.GetCustomAttribute<T>() != null;

        public T GetRequiredAttribute<T>() where T : Attribute =>
            self.GetCustomAttribute<T>() ?? throw new ArgumentException($"{typeof(T)} not found on {self}.");
    }

    extension (Type self)
    {
        public IEnumerable<Type> BaseTypes
        {
            get
            {
                foreach (var i in self.GetInterfaces())
                {
                    yield return i;
                }

                Type? currentBaseType = self.BaseType;
                while (currentBaseType != null)
                {
                    yield return currentBaseType;
                    currentBaseType = currentBaseType.BaseType;
                }
            }
        }
    }
}
