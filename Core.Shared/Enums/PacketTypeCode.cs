namespace Soteo.Core.Shared.Enums;

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
    Pause,
    CharacterRecalled,
    Move,
    UseAbility,
    Stop,
    ShardSnapshotRequest,
    ShardSnapshot,
    ShardSnapshotDelta,
}
