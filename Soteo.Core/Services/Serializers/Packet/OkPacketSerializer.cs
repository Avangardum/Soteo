using Soteo.Core.Interfaces;
using Soteo.Core.Packets;

namespace Soteo.Core.PacketSerializers;

public sealed class OkPacketSerializer(ISerializationHelper s) : EmptyPacketSerializer<OkPacket>(s);