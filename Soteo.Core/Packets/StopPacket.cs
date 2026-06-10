using Soteo.Core.Attributes;
using Soteo.Core.Commands;
using Soteo.Core.Enums;

namespace Soteo.Core.Packets;

[PacketTypeCode(PacketTypeCode.Stop)]
public sealed record StopPacket : CommandPacket<StopCommand>;