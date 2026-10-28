namespace Soteo.Main.Gameplay.Ui;

public sealed partial class HudNode : Control
{
    private static readonly PackedScene Scene = ResourceLoader.Load<PackedScene>("res://Scenes/Ui/Hud.tscn");

    public Hud? Hud { get; set; }

    private HudNode() { }

    public static HudNode Instance() => Scene.Instantiate<HudNode>(); // todo rename

    public override void _Process(double delta) => Hud?.Process(delta);
}
