namespace Soteo.Core.Shared.Enums;

public enum PacketTypeCode : byte
{
    // Shared
    Chunk,
    Ok,
    BadInput,
    Ping,
    Pause,
    // Campaign server <--> Client / Shard server
    CampaignServerHandshake,
    SpawnCharacter,
    CharacterRecalled,
    WebrtcSdp,
    WebrtcIceCandidate,
    // Client --> Shard server
    Move,
    UseAbility,
    Stop,
    ShardSnapshotRequest,
    // Shard Server --> Client
    ShardSnapshot,
    ShardSnapshotDelta,
}
