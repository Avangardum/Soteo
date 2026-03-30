namespace Soteo.Shared.Enums;

public enum MessageType : byte
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
