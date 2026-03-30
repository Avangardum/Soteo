using Soteo.Shared.Attributes;
using Soteo.Shared.Enums;
using Soteo.Shared.Packets.Shared;

namespace Soteo.Shared.Packets.Master;

[PacketType(PacketType.Ok)]
public sealed record OkPacket : Packet;