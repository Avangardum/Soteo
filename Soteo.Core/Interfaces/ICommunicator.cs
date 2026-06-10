namespace Soteo.Core.CampaignServer.Interfaces;

public interface ICommunicator : IFromCampaignServerPacketSender
{
    void Poll();
}
