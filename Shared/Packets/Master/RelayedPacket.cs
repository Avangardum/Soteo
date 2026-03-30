using Soteo.Shared.Packets.Shared;

namespace Soteo.Shared.Packets.Master;

public abstract record RelayedPacket : Packet
{
    public Guid PeerId { get; set; }
}