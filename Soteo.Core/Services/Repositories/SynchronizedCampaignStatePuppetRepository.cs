using Soteo.Core.Dto;
using Soteo.Core.Dto.Packets;
using Soteo.Core.Interfaces;

namespace Soteo.Core.Services.Repositories;

/// <inheritdoc cref="ISynchronizedCampaignStateRepository" />
public sealed class SynchronizedCampaignStatePuppetRepository :
    ISynchronizedCampaignStatePuppetRepository,
    ISynchronizedCampaignStatePacketReceiver,
    IPauseRepository
{
    public event Action Changed = delegate {};
    
    private SynchronizedCampaignState? _value;
    
    /// <inheritdoc/>
    public SynchronizedCampaignState Value
    {
        get
        {
            return _value ??
                throw new InvalidOperationException("This API should not be used until the app is initialized");
        }
    }

    public void ReceiveSynchronizedCampaignStatePacket(SynchronizedCampaignStatePacket packet)
    {
        if (_value == packet.Value) return;
        _value = packet.Value;
        Changed();
    }
    
    public bool IsPaused => Value.IsPaused;
}
