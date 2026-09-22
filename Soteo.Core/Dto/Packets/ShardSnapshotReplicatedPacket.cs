using Soteo.Core.Attributes;
using Soteo.Core.Enums;

namespace Soteo.Core.Dto.Packets;

[EmptyPacket]
public sealed record ShardSnapshotReplicatedPacket : Packet;
