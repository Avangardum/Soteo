using Soteo.Core.Dto.Packets;
using Soteo.Core.Interfaces;

namespace Soteo.TestUtil;

public sealed class FakeFromGameplayPacketSender : IFromGameplayPacketSender
{
    private readonly Dictionary<Guid, List<Packet>> _personalPackets = [];
    private readonly List<Packet> _broadcastPackets = [];
    
    public void SendReliable(Packet packet, params IEnumerable<Guid> receiverIds)
    {
        foreach (Guid id in receiverIds)
        {
            if (!_personalPackets.ContainsKey(id))
                _personalPackets[id] = [];
            _personalPackets[id].Add(packet);
        }
    }

    public void SendUnreliable(Packet packet, params IEnumerable<Guid> receiverIds) =>
        SendReliable(packet, receiverIds);

    public void BroadcastReliable(Packet packet) => _broadcastPackets.Add(packet);

    public void BroadcastUnreliable(Packet packet) => BroadcastReliable(packet);
    
    public IReadOnlyList<Packet> PacketsSentTo(Guid id)
    {
        if (!_personalPackets.ContainsKey(id))
            _personalPackets[id] = [];
        return [.._personalPackets[id], .._broadcastPackets];
    }
}
