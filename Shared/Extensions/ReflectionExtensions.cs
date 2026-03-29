using System.Reflection;
using Soteo.Shared.Attributes;
using Soteo.Shared.Enums;

namespace Soteo.Shared.Extensions;

public static class ReflectionExtensions
{
    extension(Type self)
    {
        public bool HasAttribute<T>() where T : Attribute =>
            CustomAttributeExtensions.GetCustomAttribute<T>(self) != null;
        
        public T? GetCustomAttribute<T>() where T : Attribute =>
            (T)self.GetCustomAttribute(typeof(T));
        
        public T GetRequiredAttribute<T>() where T : Attribute =>
            self.GetCustomAttribute<T>() ?? throw new ArgumentException($"{typeof(T)} not found on {self}.");
        
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
        
        public bool IsAssignableTo(Type other) => other.IsAssignableFrom(self);
        
        public MessageType GetMessageType(Type baseGenericClass)
        {
            return self.BaseTypes
                .Single(bt => bt.IsConstructedGenericType && bt.GetGenericTypeDefinition() == baseGenericClass)
                .GenericTypeArguments.Single()
                .GetRequiredAttribute<MessageTypeAttribute>().Type;
        }
    }
}