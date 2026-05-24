using Soteo.Core.Shared.Packets;

namespace Soteo.Gameplay.Interfaces;

public interface IChunkCollector
{
    byte[]? AddChunk(ChunkPacket chunk, Guid senderId);
}