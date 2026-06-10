using Soteo.Core.Attributes;
using Soteo.Core.Enums;

namespace Soteo.Core.Extensions;

public static class ReflectionExtensions
{
    extension (Type self)
    {
        public PacketTypeCode GetPacketType(Type baseGenericClassDefinition)
        {
            if (!baseGenericClassDefinition.IsGenericTypeDefinition)
                throw new ArgumentException($"{baseGenericClassDefinition} is not a generic class definition");
            if (baseGenericClassDefinition.GetGenericArguments().Length != 1)
                throw new ArgumentException($"{baseGenericClassDefinition} doesn't have exactly 1 generic argument");
            Type? baseGenericClass = self.BaseTypes
                .SingleOrDefault
                (
                    bt => bt.IsConstructedGenericType && bt.GetGenericTypeDefinition() == baseGenericClassDefinition
                );
            if (baseGenericClass == null)
                throw new ArgumentException($"{self} is not derived from {baseGenericClassDefinition}");
            return baseGenericClass.GenericTypeArguments.Single().GetRequiredAttribute<PacketTypeCodeAttribute>().TypeCode;
        }
    }
}
