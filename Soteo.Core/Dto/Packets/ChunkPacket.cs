using Soteo.Core.Attributes;
using Soteo.Core.Enums;

namespace Soteo.Core.Packets;

/// <summary>
/// Fragment of a chunked packet
/// </summary>
[PacketTypeCode(PacketTypeCode.Chunk)]
public sealed record ChunkPacket : Packet
{
    public required Guid GroupId { get; init; }
    public required int Index { get; init; }
    public required bool IsLast { get; init; }
    public required byte[] Bytes { get; init; }
}
