using System.Diagnostics;
using Soteo.Core.Enums;
using Soteo.Core.Interfaces;

namespace Soteo.Main.Shared;

public sealed class FrameStopwatch : IFrameStopwatch
{
    private readonly Stopwatch _processStopwatch = new();
    private readonly Stopwatch _physicsProcessStopwatch = new();

    public FrameStopwatch(IProcessPublisher processPublisher)
    {
        processPublisher.SubscribeToProcess
        (
            _processStopwatch.Restart,
            ProcessPriorityEnum.FrameStopwatch,
            callWhenPaused: true
        );
        processPublisher.SubscribeToPhysicsProcess
        (
            _physicsProcessStopwatch.Restart,
            ProcessPriorityEnum.FrameStopwatch,
            callWhenPaused: true
        );
    }
    
    public double ElapsedSinceProcess => _processStopwatch.Elapsed.TotalSeconds;
    public double ElapsedSincePhysicsProcess => _physicsProcessStopwatch.Elapsed.TotalSeconds;
}
