using System.Diagnostics;
using Soteo.Core;

namespace Soteo.Main;

public sealed class FrameStopwatch : IFrameStopwatch
{
    private readonly Stopwatch _processStopwatch = new();
    private readonly Stopwatch _physicsProcessStopwatch = new();

    public FrameStopwatch(IProcessPublisher processPublisher)
    {
        processPublisher.SubscribeToProcess
        (
            _processStopwatch.Restart,
            ProcessPriority.FrameStopwatch,
            callWhenPaused: true
        );
        processPublisher.SubscribeToPhysicsProcess
        (
            _physicsProcessStopwatch.Restart,
            ProcessPriority.FrameStopwatch,
            callWhenPaused: true
        );
    }

    public double ElapsedSinceProcess => _processStopwatch.Elapsed.TotalSeconds;
    public double ElapsedSincePhysicsProcess => _physicsProcessStopwatch.Elapsed.TotalSeconds;
}
