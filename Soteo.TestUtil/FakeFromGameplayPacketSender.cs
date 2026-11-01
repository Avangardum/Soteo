using Soteo.Core.Dto.Packets;
using Soteo.Core.Interfaces;

namespace Soteo.TestUtil;

public sealed class FakeFromGameplayPacketSender : IFromGameplayPacketSender
{
    private readonly Dictionary<Guid, List<Packet>> _personalPackets = [];
    private readonly List<Packet> _broadcastPackets = [];

    public void Send(Packet packet, IReadOnlyList<Guid> receiverIds, bool reliable = true, bool deferToNextTick = false)
    {
        foreach (Guid id in receiverIds)
        {
            if (!_personalPackets.ContainsKey(id))
                _personalPackets[id] = [];
            _personalPackets[id].Add(packet);
        }
    }

    public void Broadcast(Packet packet, bool reliable = true, bool deferToNextTick = false) =>
        _broadcastPackets.Add(packet);

    public IReadOnlyList<Packet> PacketsSentTo(Guid id)
    {
        if (!_personalPackets.ContainsKey(id))
            _personalPackets[id] = [];
        return [.._personalPackets[id], .._broadcastPackets];
    }
}
