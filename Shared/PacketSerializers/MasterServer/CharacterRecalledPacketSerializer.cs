using Soteo.Shared.Packets.MasterServer;
using Soteo.Shared.PacketSerializers.Shared;

namespace Soteo.Shared.PacketSerializers.MasterServer;

public sealed class CharacterRecalledPacketSerializer : PacketSerializer<CharacterRecalledPacket>
{
    protected override int PacketSize(CharacterRecalledPacket packet)
    {
        return base.PacketSize(packet) + SizeOf(packet.CharacterId);
    }

    protected override void SerializeInternal(CharacterRecalledPacket packet, ref Span<byte> span)
    {
        base.SerializeInternal(packet, ref span);
        SerializeGuid(packet.CharacterId, ref span);
    }

    protected override CharacterRecalledPacket DeserializeInternal(ref Span<byte> span)
    {
        var packet = base.DeserializeInternal(ref span);
        packet.CharacterId = DeserializeGuid(ref span);
        return packet;
    }
}