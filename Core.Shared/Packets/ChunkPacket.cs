using Soteo.Core.Shared.Attributes;
using Soteo.Core.Shared.Enums;

namespace Soteo.Core.Shared.Packets;

/// <summary>
/// Fragment of a chunked packet
/// </summary>
[PacketType(PacketType.Chunk)]
public sealed record ChunkPacket : Packet
{
    public Guid GroupId { get; set; }
    public int Index { get; set; }
    public bool IsLast { get; set; }
    public byte[] Bytes { get; set; } = [];
}
