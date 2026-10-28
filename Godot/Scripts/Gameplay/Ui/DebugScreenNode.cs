namespace Soteo.Main.Gameplay.Ui;

public sealed partial class DebugScreenNode : Control
{
    private static readonly PackedScene Scene = ResourceLoader.Load<PackedScene>("res://Scenes/Ui/DebugScreen.tscn");

    public DebugScreen? DebugScreen { get; set; }

    private DebugScreenNode() { }

    public static DebugScreenNode Instance() => Scene.Instantiate<DebugScreenNode>(); // todo rename

    public override void _PhysicsProcess(double delta) => DebugScreen?.PhysicsProcess(delta);
    public override void _Process(double delta) => DebugScreen?.Process(delta);
    public override void _UnhandledInput(InputEvent e) => DebugScreen?.UnhandledInput(e);
}
