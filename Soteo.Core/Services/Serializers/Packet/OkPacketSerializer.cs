using Soteo.Core.Dto.Packets;
using Soteo.Core.Interfaces;

namespace Soteo.Core.Services.Serializers.Packet;

public sealed class OkPacketSerializer(ISerializationHelper s) : EmptyPacketSerializer<OkPacket>(s);