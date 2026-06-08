namespace Soteo.Core.Shared.Enums;

public enum PacketTypeCode : byte
{
    // Shared
    Chunk,
    Ok,
    BadInput,
    Ping,
    Pause,
    CampaignServerHandshake,
    SpawnCharacter,
    WebrtcSdp,
    WebrtcIceCandidate,
    
    // Campaign server --> Shard server
    PersistenceShardSnapshotRequest,
    
    // Shard server --> Campaign server
    PersistenceShardSnapshot,
    CharacterRecalled,
    
    // Client --> Shard server
    Move,
    UseAbility,
    Stop,
    SynchronizationShardSnapshotRequest,
    
    // Shard server --> Client
    SynchronizationShardSnapshot,
    ShardSnapshotDelta,
}
