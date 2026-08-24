namespace Soteo.Core.Enums;

// todo dynamic type codes
public enum PacketTypeCode : byte
{
    Chunk,
    Ok,
    BadInput,
    Ping,
    CampaignServerHandshake,
    SpawnCharacter,
    WebrtcSdp,
    WebrtcIceCandidate,
    CharacterRecalled,
    Move,
    UseAbility,
    Stop,
    ShardSnapshotRequest,
    ShardSnapshot,
    ShardSnapshotDelta,
    ShardSnapshotReplicated,
    SynchronizedCampaignState,
    NoInitialShardSnapshot,
    CampaignInitialized,
    ShardServerInitAwaitingCampaignServerInit,
}
