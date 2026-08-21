namespace Soteo.Core.Interfaces;

public interface IFromCampaignServerCommunicator : IFromCampaignServerPacketSender, IConnectionNotifier
{
    bool AllowPlayerConnections { get; set; }
    void Poll();
}
