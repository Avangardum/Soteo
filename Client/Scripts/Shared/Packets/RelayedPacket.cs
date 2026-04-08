namespace Soteo.Shared.Packets;

public abstract record RelayedPacket : Packet
{
    public Guid PeerId { get; set; }
}