using Soteo.Shared.Packets.MasterServer;

namespace Soteo.Shared.PacketSerializers.MasterServer;

public sealed class SpawnCharacterPacketSerializer : RelayedPacketSerializer<SpawnCharacterPacket>
{
    protected override int PacketSize(SpawnCharacterPacket packet)
    {
        return base.PacketSize(packet) + SizeOf(packet.SpawnPointId);
    }

    protected override void SerializeInternal(SpawnCharacterPacket packet, ref Span<byte> span)
    {
        base.SerializeInternal(packet, ref span);
        SerializeGuid(packet.SpawnPointId, ref span);
    }

    protected override SpawnCharacterPacket DeserializeInternal(ref Span<byte> span)
    {
        var packet = base.DeserializeInternal(ref span);
        packet.SpawnPointId = DeserializeGuid(ref span);
        return packet;
    }
}