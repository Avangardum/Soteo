using Soteo.Shared.Attributes;
using Soteo.Shared.Enums;

namespace Soteo.Shared.Packets.Shared;

[PacketType(PacketType.InternalServerError)]
public sealed record InternalServerErrorPacket : Packet;