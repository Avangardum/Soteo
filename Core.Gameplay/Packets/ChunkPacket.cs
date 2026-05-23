using Soteo.Shared.Attributes;
using Soteo.Shared.Enums;

namespace Soteo.Shared.Packets;

[PacketType(PacketType.Chunk)]
public sealed record ChunkPacket : Packet
{
    public Guid GroupId { get; set; }
    public int Index { get; set; }
    public bool IsLast { get; set; }
    public byte[] Bytes { get; set; } = [];
}