namespace Soteo.Core.Shared.Packets;

public abstract record RelayedPacket : Packet
{
    public Guid PeerId { get; set; }
}