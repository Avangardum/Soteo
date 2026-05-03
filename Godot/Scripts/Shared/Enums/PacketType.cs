namespace Soteo.Shared.Enums;

public enum PacketType : byte
{
    // Shared
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
    // Shard Server --> Client
    ShardSnapshot
}
