namespace Soteo.Shared.Enums;

public enum PacketType : byte
{
    // Shared
    Ok,
    BadInput,
    InternalServerError,
    // Master server <--> *
    Handshake,
    SpawnCharacter,
    CharacterRecalled,
    WebrtcSdp,
    WebrtcIceCandidate,
    // Client <--> Shard
    Move,
    Attack,
    UseAbility
}
