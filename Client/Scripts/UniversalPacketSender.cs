using Soteo.Shared.Packets.Shared;

namespace Soteo.Client;

public sealed class UniversalPacketSender
(
    IMasterServerCommunicator masterSender,
    IPacketSender clientShardServerSender
) : IPacketSender
{
    public void SendReliable(Packet packet, Guid receiverId)
    {
        if (receiverId == MasterServerId) masterSender.SendPacket(packet);
        else clientShardServerSender.SendReliable(packet, receiverId);
    }
}