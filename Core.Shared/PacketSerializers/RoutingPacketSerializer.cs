using Soteo.Core.Shared.Enums;
using Soteo.Core.Shared.Interfaces;
using Soteo.Core.Shared.Packets;

namespace Soteo.Core.Shared.PacketSerializers;

public sealed class RoutingPacketSerializer : IPacketSerializer
{
    public Packet Deserialize(Span<byte> bytes) =>
        PacketSerializer.For((PacketTypeCode)bytes[0]).Deserialize(bytes);

    public byte[] Serialize(Packet packet) =>
        PacketSerializer.For(packet.TypeCode).Serialize(packet);
}
