namespace Soteo.Core.Packets;

public sealed record ShardDeltaPacket : Packet
{
    public required double ServerLoad { get; init; }
    public required ShardDelta Delta { get; init; }
}
