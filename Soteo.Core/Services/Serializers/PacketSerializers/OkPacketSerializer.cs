using Soteo.Core.Dto.Packets;
using Soteo.Core.Interfaces;

namespace Soteo.Core.Services.Serializers.PacketSerializers;

public sealed class OkPacketSerializer(ISerializationHelper s) : EmptyPacketSerializer<OkPacket>(s);