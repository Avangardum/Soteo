using System.Diagnostics;
using Soteo.Gameplay.Enums;

namespace Soteo.Shared.Nodes.Autoloads;

/// <summary>
/// Counts time since current frame beginning
/// </summary>
public sealed class FrameStopwatch : Node
{
    private readonly Stopwatch _processStopwatch = new();
    private readonly Stopwatch _physicsProcessStopwatch = new();
    
    public double ElapsedSinceProcess => _processStopwatch.Elapsed.TotalSeconds;
    public double ElapsedSincePhysicsProcess => _physicsProcessStopwatch.Elapsed.TotalSeconds;
    
    public static FrameStopwatch Instance { get; private set; } = null!;
    
    public override void _Ready()
    {
        ProcessPriority = (int)ProcessPriorityEnum.FrameStopwatch;
        Instance = this;
    }

    public override void _Process(float delta) => _processStopwatch.Restart();
    
    public override void _PhysicsProcess(float delta) => _physicsProcessStopwatch.Restart();
}