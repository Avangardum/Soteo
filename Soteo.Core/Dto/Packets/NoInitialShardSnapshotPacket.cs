using Soteo.Core.Attributes;
using Soteo.Core.Enums;

namespace Soteo.Core.Dto.Packets;

/// <summary>
/// Sent from the campaign server to a shard server to notify
/// that no shard snapshot needs to be replicated during initialization.
/// </summary>
[PacketTypeCode(PacketTypeCode.NoInitialShardSnapshot)]
public sealed record NoInitialShardSnapshotPacket : Packet;
