namespace Soteo.Shared.Enums;

public enum PacketType : byte
{
    // Shared
    Ok,
    BadInput,
    Ping,
    // Master server <--> Client / Shard server
    MasterServerHandshake,
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
