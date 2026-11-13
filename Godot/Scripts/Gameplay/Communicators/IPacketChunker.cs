using Soteo.Core.Packets;

namespace Soteo.Main.Gameplay.Communicators;

public interface IPacketChunker
{
    byte[][] SplitIntoChunks(Packet packet, int maxChunkSize);
}
