namespace Soteo.Core.Interfaces;

public interface IFromCampaignServerCommunicator : IFromCampaignServerPacketSender
{
    bool AllowPlayerConnections { get; set; }
    void Poll();
}
