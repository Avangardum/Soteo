using System.Diagnostics;
using Soteo.Core.Gameplay.Interfaces;
using Soteo.Shared.Interfaces;

namespace Soteo.Shared.Nodes.Autoloads;

public sealed class FrameStopwatch : IFrameStopwatch, IDisposable
{
    private readonly Stopwatch _processStopwatch = new();
    private readonly Stopwatch _physicsProcessStopwatch = new();
    private readonly IDisposable _processSubscriptions;

    public FrameStopwatch(IProcessPublisher processPublisher)
    {
        IDisposable processSubscription = processPublisher.SubscribeToProcess(_processStopwatch.Restart);
        IDisposable physicsProcessSubscription =
            processPublisher.SubscribeToPhysicsProcess(_physicsProcessStopwatch.Restart);
        _processSubscriptions = new AggregateDisposable(processSubscription, physicsProcessSubscription);
    }
    
    public void Dispose()
    {
        _processSubscriptions.Dispose();
    }
    
    public double ElapsedSinceProcess => _processStopwatch.Elapsed.TotalSeconds;
    public double ElapsedSincePhysicsProcess => _physicsProcessStopwatch.Elapsed.TotalSeconds;
}
