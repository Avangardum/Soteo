using Soteo.Core.Dto.Packets;

namespace Soteo.Main.Gameplay.Interfaces;

public interface IPacketChunker
{
    byte[][] SplitIntoChunks(Packet packet, int maxChunkSize);
}
