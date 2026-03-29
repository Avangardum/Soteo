namespace Soteo.Shared.Enums;

public enum MessageType : byte
{
    Ok,
    BadInput,
    InternalServerError,
    SpawnCharacter,
    CharacterRecalled,
    WebrtcSdp,
    WebrtcIceCandidate,
    Handshake
}
