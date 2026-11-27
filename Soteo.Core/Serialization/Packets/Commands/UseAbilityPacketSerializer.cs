using Soteo.Core.Commands;
using Soteo.Core.Packets.Commands;

namespace Soteo.Core.Serialization.Packets.Commands;

public sealed class UseAbilityPacketSerializer(SerializationHelper s) : PacketSerializer<UseAbilityPacket>(s)
{
    protected override void SerializeCore(UseAbilityPacket packet, Stream stream)
    {
        s.SerializeGuid(packet.UnitId, stream);
        s.SerializeAbilitySlot(packet.Command.Slot, stream);
        s.SerializeBool(packet.Command.Repeat, stream);
        s.SerializeBool(packet.Command.Alt, stream);
        s.SerializeAbilityTargetSnapshot(packet.Command.Target, stream);
    }

    protected override UseAbilityPacket DeserializeCore(Stream stream)
    {
        return new UseAbilityPacket
        {
            UnitId = s.DeserializeGuid(stream),
            Command = new UseAbilityCommand
            {
                Slot = s.DeserializeAbilitySlot(stream),
                Repeat = s.DeserializeBool(stream),
                Alt = s.DeserializeBool(stream),
                Target = s.DeserializeAbilityTargetSnapshot(stream)
            }
        };
    }
}
