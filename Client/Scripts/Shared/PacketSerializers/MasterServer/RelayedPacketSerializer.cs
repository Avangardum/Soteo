using Soteo.Shared.Packets.MasterServer;
using Soteo.Shared.PacketSerializers.Shared;

namespace Soteo.Shared.PacketSerializers.MasterServer;

public abstract class RelayedPacketSerializer<TPacket> : PacketSerializer<TPacket>
    where TPacket : RelayedPacket, new()
{
    protected override int PacketSize(TPacket packet) => base.PacketSize(packet) + Const.BytesInGuid;

    protected override void SerializeInternal(TPacket packet, ref Span<byte> span)
    {
        base.SerializeInternal(packet, ref span);
        SerializeGuid(packet.PeerId, ref span);
    }

    protected override TPacket DeserializeInternal(ref Span<byte> span)
    {
        TPacket packet = base.DeserializeInternal(ref span);
        packet.PeerId = DeserializeGuid(ref span);
        return packet;
    }
}