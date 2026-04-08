namespace Soteo.Shared.Enums;

public enum PacketType : byte
{
    // Shared
    Ok,
    BadInput,
    // Master server <--> Client / Shard server
    MasterServerHandshake,
    SpawnCharacter,
    CharacterRecalled,
    WebrtcSdp,
    WebrtcIceCandidate,
    // Client --> Shard server
    Move,
    Attack,
    UseAbility,
    // Shard Server --> Client
    ShardSnapshot
}
