using Soteo.Core.Dto.Packets;

namespace Soteo.Main.Gameplay.Interfaces;

public interface IPacketChunkCollector
{
    /// <summary>Add a chunk to the collector.</summary>
    /// <returns>Restored packet if the added chunk was the last one needed to restore it. Null otherwise.</returns>
    byte[]? AddChunk(ChunkPacket chunk, Guid senderId);
}
