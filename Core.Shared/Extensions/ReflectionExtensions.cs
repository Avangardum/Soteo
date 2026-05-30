using Soteo.Core.Shared.Attributes;
using Soteo.Core.Shared.Enums;

namespace Soteo.Core.Shared.Extensions;

public static class ReflectionExtensions
{
    extension (Type self)
    {
        public PacketType GetPacketType(Type baseGenericClass)
        {
            return self.BaseTypes
                .Single(bt => bt.IsConstructedGenericType && bt.GetGenericTypeDefinition() == baseGenericClass)
                .GenericTypeArguments.Single().GetRequiredAttribute<PacketTypeAttribute>().Type;
        }
    }
}
