using Soteo.Core.Dto.Packets;

namespace Soteo.Core.Extensions;

public static class ReflectionExtensions
{
    extension (Type self)
    {
        /// <summary>
        /// For a type inheriting from a base generic type with a single type parameter being a packet type
        /// (PacketSerializer, PacketHandler), get the packet type
        /// </summary>
        public Type GetPacketType(Type baseGenericClassDefinition)
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
            Type packetType = baseGenericClass.GenericTypeArguments.Single();
            if (!packetType.IsAssignableTo(typeof(Packet)))
                throw new ArgumentException($"The generic argument {packetType} is not a packet");
            return packetType;
        }
    }
}
