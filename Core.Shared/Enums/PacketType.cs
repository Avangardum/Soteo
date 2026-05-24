namespace Soteo.Shared.Enums;

public enum PacketType : byte
{
    // Shared
    Chunk,
    Ok,
    BadInput,
    Ping,
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
