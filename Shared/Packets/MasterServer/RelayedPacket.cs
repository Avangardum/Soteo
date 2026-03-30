using Soteo.Shared.Packets.Shared;

namespace Soteo.Shared.Packets.MasterServer;

public abstract record RelayedPacket : Packet
{
    public Guid PeerId { get; set; }
}