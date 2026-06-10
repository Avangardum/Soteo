namespace Soteo.Core.CampaignServer.Interfaces;

public interface ICommunicator : IPacketSender
{
    void Poll();
}
