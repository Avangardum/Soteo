using Soteo.Shared.Attributes;
using Soteo.Shared.Enums;

namespace Soteo.Shared.Packets.Shared;

[PacketType(PacketType.Ok)]
public sealed record OkPacket : Packet;