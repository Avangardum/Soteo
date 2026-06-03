using Soteo.Core.Shared.Attributes;
using Soteo.Core.Shared.Enums;

namespace Soteo.Core.Shared.Packets;

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
