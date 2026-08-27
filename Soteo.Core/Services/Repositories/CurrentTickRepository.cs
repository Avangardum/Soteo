using Soteo.Core.Enums;
using Soteo.Core.Interfaces;

namespace Soteo.Core.Services.Repositories;

/// <summary>
/// Stores current tick on a shard server
/// </summary>
public sealed class CurrentTickRepository : ICurrentTickRepository, IDisposable
{
    private readonly IDisposable _physicsProcessSubscription;
    
    public long Tick { get; set; }

    public CurrentTickRepository(IProcessPublisher processPublisher)
    {
        _physicsProcessSubscription = processPublisher
            .SubscribeToPhysicsProcess(() => Tick++, ProcessPriorityEnum.CurrentTickRepository, callWhenPaused: false);
    }
        
    public void Dispose()
    {
        _physicsProcessSubscription.Dispose();
    }
}
