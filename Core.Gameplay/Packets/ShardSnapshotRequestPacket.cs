using Soteo.Core.Shared.Attributes;
using Soteo.Core.Shared.Enums;
using Soteo.Core.Shared.Packets;

namespace Soteo.Core.Gameplay.Packets;

[PacketType(PacketType.ShardSnapshotRequest)]
public sealed record ShardSnapshotRequestPacket : Packet;
