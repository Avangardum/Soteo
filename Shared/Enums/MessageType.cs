namespace Soteo.Shared.Enums;

public enum MessageType : byte
{
    Ok,
    InvalidMessage,
    SpawnCharacter,
    CharacterRecalled,
    WebrtcSdp,
    WebrtcIceCandidate,
    Handshake
}
