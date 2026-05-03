namespace Soteo.CampaignServer.Interfaces;

public interface ICommunicator : IPacketSender
{
    void Poll();
}