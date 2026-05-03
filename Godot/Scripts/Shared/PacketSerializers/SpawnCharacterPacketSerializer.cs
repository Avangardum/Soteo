using Soteo.Shared.Packets;

namespace Soteo.Shared.PacketSerializers;

public sealed class SpawnCharacterPacketSerializer : RelayedPacketSerializer<SpawnCharacterPacket>
{
    protected override void SerializeInternal(SpawnCharacterPacket packet, Stream stream)
    {
        base.SerializeInternal(packet, stream);
        SerializeGuid(packet.SpawnPointId, stream);
    }

    protected override SpawnCharacterPacket DeserializeInternal(Stream stream)
    {
        var packet = base.DeserializeInternal(stream);
        packet.SpawnPointId = DeserializeGuid(stream);
        return packet;
    }
}