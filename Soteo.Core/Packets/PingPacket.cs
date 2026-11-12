namespace Soteo.Core.Packets;

public sealed record PingPacket : Packet
{
    public required Guid Id { get; init; }
    public required bool IsResponse { get; init; }
}
