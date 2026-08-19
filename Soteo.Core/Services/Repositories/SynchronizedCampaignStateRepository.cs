using Soteo.Core.Dto;
using Soteo.Core.Dto.Packets;
using Soteo.Core.Enums;
using Soteo.Core.Interfaces;
using Soteo.Core.StaticHelpers;

namespace Soteo.Core.Services.Repositories;

/// <inheritdoc/>
public sealed class SynchronizedCampaignStateRepository : ISynchronizedCampaignStateRepository
{
    private readonly IFromCampaignServerPacketSender _packetSender;
    
    private bool _isChanged;
    private readonly HashSet<Guid> _valueRequesterIds = [];

    public SynchronizedCampaignStateRepository
    (
        IProcessPublisher processPublisher,
        IFromCampaignServerPacketSender packetSender,
        IConnectionNotifier connectionNotifier
    )
    {
        _packetSender = packetSender;
        connectionNotifier.PeerConnected += OnPeerConnected;
        
        processPublisher
            .SubscribeToPhysicsProcess(Tick, ProcessPriorityEnum.SynchronizationServer, callWhenPaused: true);
    }
    
    public SynchronizedCampaignState Value
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            _isChanged = true;
        }
    } = new();
    
    private void Tick()
    {
        if (_isChanged)
            _packetSender.BroadcastToAll(new SynchronizedCampaignStatePacket(Value));
        else if (_valueRequesterIds.Any())
            _packetSender.SendTo(new SynchronizedCampaignStatePacket(Value), _valueRequesterIds);
        _isChanged = false;
        _valueRequesterIds.Clear();
    }
    
    private void OnPeerConnected(Guid id)
    {
        if (id != Const.CampaignServerId)
            _valueRequesterIds.Add(id);
    }
}
