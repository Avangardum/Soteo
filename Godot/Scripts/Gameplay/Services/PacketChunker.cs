using AwesomeAssertions;
using Soteo.Core.Dto.Packets;
using Soteo.Core.Interfaces;
using Soteo.Main.Gameplay.Interfaces;
using Soteo.Util;

namespace Soteo.Main.Gameplay.Services;

public sealed class PacketChunker : IPacketChunker
{
    private readonly IPacketSerializer _packetSerializer;
    private readonly int _emptyChunkPacketSize;

    public PacketChunker(IPacketSerializer packetSerializer)
    {
        _packetSerializer = packetSerializer;
        var emptyChunkPacket = new ChunkPacket
        {
            GroupId = Guid.Empty,
            Index = 0,
            IsLast = false,
            Bytes = []
        };
        _emptyChunkPacketSize = packetSerializer.Serialize(emptyChunkPacket).Length;
    }

    public byte[][] SplitIntoChunks(Packet packet, int maxChunkSize)
    {
        if (maxChunkSize < 100) throw new ArgumentException("Max chunk size should be at least 100");
        int maxBodySize = maxChunkSize - _emptyChunkPacketSize;
        maxBodySize.Should().BeGreaterThanOrEqualTo(10);
        byte[] bytes = _packetSerializer.Serialize(packet);
        if (bytes.Length <= maxChunkSize) return [bytes];

        var groupId = Guid.NewGuid();
        var chunks = new byte[Maths.CeilToInt((double)bytes.Length / maxBodySize)][];
        for (int i = 0; i < chunks.Length; i++)
        {
            bool isLast = i == chunks.Length - 1;
            int start = i * maxBodySize;
            int end = isLast ? bytes.Length : start + maxBodySize;
            var chunkPacket = new ChunkPacket
            {
                GroupId = groupId,
                Index = i,
                IsLast = isLast,
                Bytes = bytes[start..end].ToArray()
            };
            chunks[i] = _packetSerializer.Serialize(chunkPacket);
            chunks[i].Length.Should().BeLessThanOrEqualTo(maxChunkSize);
        }
        return chunks;
    }
}
