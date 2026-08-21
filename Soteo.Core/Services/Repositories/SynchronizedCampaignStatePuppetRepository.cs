using Soteo.Core.Dto;
using Soteo.Core.Dto.Packets;
using Soteo.Core.Interfaces;

namespace Soteo.Core.Services.Repositories;

/// <inheritdoc cref="ISynchronizedCampaignStateRepository" />
public sealed class SynchronizedCampaignStatePuppetRepository :
    ISynchronizedCampaignStatePuppetRepository,
    ISynchronizedCampaignStatePacketReceiver
{
    public event Action Changed = delegate {};
    
    private SynchronizedCampaignState? _value;
    
    /// <inheritdoc/>
    public SynchronizedCampaignState Value
    {
        get
        {
            return _value ??
                throw new InvalidOperationException("SynchronizedCampaignState is not yet received");
        }
    }
    
    public bool IsInitialized { get; private set; }

    public void ReceiveSynchronizedCampaignStatePacket(SynchronizedCampaignStatePacket packet)
    {
        if (_value == packet.Value) return;
        _value = packet.Value;
        IsInitialized = true;
        Changed();
    }
}
