using Soteo.Shared.Attributes;
using Soteo.Shared.Enums;

namespace Soteo.Shared.Packets;

[PacketType(PacketType.ShardSnapshotRequest)]
public sealed record ShardSnapshotRequestPacket : Packet;
