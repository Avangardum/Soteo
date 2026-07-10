using Soteo.Core.Attributes;
using Soteo.Core.Enums;

namespace Soteo.Core.Dto.Packets;

[PacketTypeCode(PacketTypeCode.ShardSnapshotReplicated)]
public sealed record ShardSnapshotReplicatedPacket : Packet;
