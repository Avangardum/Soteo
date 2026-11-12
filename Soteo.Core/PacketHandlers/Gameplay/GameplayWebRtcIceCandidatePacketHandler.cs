namespace Soteo.Core.PacketHandlers.Gameplay;

[GameplayPacketHandler]
public sealed class GameplayWebRtcIceCandidatePacketHandler(IWebrtcPacketReceiver receiver) :
    PacketHandler<WebrtcIceCandidatePacket>
{
    protected override void Handle(WebrtcIceCandidatePacket packet, Guid senderId)
    {
        receiver.ReceiveWebrtcIceCandidatePacket(packet);
    }
}
