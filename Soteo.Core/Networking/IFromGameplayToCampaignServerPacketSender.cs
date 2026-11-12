namespace Soteo.Core.Networking;

public interface IFromGameplayToCampaignServerPacketSender
{
    void SendPacket(Packet packet);
}
