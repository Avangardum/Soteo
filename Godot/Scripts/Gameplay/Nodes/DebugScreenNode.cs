using Soteo.Gameplay.Ui;

namespace Soteo.Gameplay.Nodes;

public sealed class DebugScreenNode : Control
{
    private static readonly PackedScene Scene = ResourceLoader.Load<PackedScene>("res://Scenes/Ui/DebugScreen.tscn");
    
    public DebugScreen? DebugScreen { get; set; }

    private DebugScreenNode() { }
    
    public static DebugScreenNode Instance() => Scene.Instance<DebugScreenNode>();
    
    public override void _PhysicsProcess(float delta) => DebugScreen?.PhysicsProcess(delta);
    public override void _Process(float delta) => DebugScreen?.Process(delta);
    public override void _UnhandledInput(InputEvent e) => DebugScreen?._UnhandledInput(e);
}
