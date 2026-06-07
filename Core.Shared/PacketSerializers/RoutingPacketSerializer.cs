using Soteo.Core.Shared.Enums;
using Soteo.Core.Shared.Interfaces;
using Soteo.Core.Shared.Packets;

namespace Soteo.Core.Shared.PacketSerializers;

public sealed class RoutingPacketSerializer(ITypeLocator typeLocator) : IPacketSerializer
{
    public Packet Deserialize(Span<byte> bytes) =>
        PacketSerializer.For((PacketTypeCode)bytes[0], typeLocator).Deserialize(bytes);

    public byte[] Serialize(Packet packet) =>
        PacketSerializer.For(packet.TypeCode, typeLocator).Serialize(packet);
}
