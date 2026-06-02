namespace Soteo.Gameplay.Ui;

public sealed class HudNode : Control
{
    private static readonly PackedScene Scene = ResourceLoader.Load<PackedScene>("res://Scenes/Ui/Hud.tscn");
    
    public Hud? Hud { get; set; }

    public static HudNode Instance() => Scene.Instance<HudNode>();
    
    public override void _Process(float delta) => Hud?.Process(delta);
}
