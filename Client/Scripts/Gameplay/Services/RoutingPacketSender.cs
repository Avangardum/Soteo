using Soteo.Gameplay.Interfaces;
using Soteo.Shared.Packets;

namespace Soteo.Gameplay.Services;

public sealed class RoutingPacketSender
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

    public void SendUnreliable(Packet packet, Guid receiverId)
    {
        if (receiverId == MasterServerId)
            throw new InvalidOperationException("Master server doesn't support unreliable messages");
        clientShardServerSender.SendUnreliable(packet, receiverId);
    }

    public void BroadcastReliable(Packet packet) => clientShardServerSender.BroadcastReliable(packet);

    public void BroadcastUnreliable(Packet packet) => clientShardServerSender.BroadcastUnreliable(packet);
}