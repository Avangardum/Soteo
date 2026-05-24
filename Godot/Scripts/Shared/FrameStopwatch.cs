using System.Diagnostics;
using Soteo.Core.Gameplay.Interfaces;
using Soteo.Gameplay.Enums;
using Soteo.Shared.Interfaces;

namespace Soteo.Shared.Nodes.Autoloads;

public sealed class FrameStopwatch : IFrameStopwatch
{
    private readonly Stopwatch _processStopwatch = new();
    private readonly Stopwatch _physicsProcessStopwatch = new();

    public FrameStopwatch(IProcessPublisher processPublisher)
    {
        processPublisher
            .SubscribeToProcess(_processStopwatch.Restart, ProcessPriorityEnum.FrameStopwatch);
        processPublisher
            .SubscribeToPhysicsProcess(_physicsProcessStopwatch.Restart, ProcessPriorityEnum.FrameStopwatch);
    }
    
    public double ElapsedSinceProcess => _processStopwatch.Elapsed.TotalSeconds;
    public double ElapsedSincePhysicsProcess => _physicsProcessStopwatch.Elapsed.TotalSeconds;
}
