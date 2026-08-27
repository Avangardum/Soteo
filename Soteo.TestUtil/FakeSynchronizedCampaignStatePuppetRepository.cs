using Soteo.Core.Dto;
using Soteo.Core.Interfaces;

namespace Soteo.TestUtil;

public sealed class FakeSynchronizedCampaignStatePuppetRepository :
    ISynchronizedCampaignStateRepository,
    ISynchronizedCampaignStatePuppetRepository
{
    public event Action Changed = delegate {};

    public SynchronizedCampaignState Value
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            Changed();
        }
    } = new();
}
