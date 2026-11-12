namespace Soteo.Core.Packets;

/// <summary>
/// Sent from the campaign server to a shard server to notify
/// that no shard snapshot needs to be replicated during initialization.
/// </summary>
[EmptyPacket]
public sealed record NoInitialShardSnapshotPacket : Packet;
