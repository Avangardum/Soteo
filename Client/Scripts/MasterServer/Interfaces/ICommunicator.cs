namespace Soteo.MasterServer.Interfaces;

public interface ICommunicator : IPacketSender
{
    void Poll();
}