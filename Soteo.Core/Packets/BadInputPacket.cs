namespace Soteo.Core.Packets;

public sealed record BadInputPacket : Packet
{
    public required string Reason { get; set; }
}
