namespace Soteo.Core.Enums;

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
}
