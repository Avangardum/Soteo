using Soteo.Shared.Attributes;
using Soteo.Shared.Enums;
using Soteo.Util.Extensions;

namespace Soteo.Shared.Extensions;

public static class ReflectionExtensions
{
    extension (Type self)
    {
        public PacketType GetPacketType(Type baseGenericClass)
        {
            return self.BaseTypes
                .Single(bt => bt.IsConstructedGenericType && bt.GetGenericTypeDefinition() == baseGenericClass)
                .GenericTypeArguments.Single()
                .GetRequiredAttribute<PacketTypeAttribute>().Type;
        }
    }
}