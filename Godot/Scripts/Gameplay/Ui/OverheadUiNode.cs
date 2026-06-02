namespace Soteo.Gameplay.Ui;

public sealed class OverheadUiNode : Control
{
    private static readonly PackedScene Scene = ResourceLoader.Load<PackedScene>("res://Scenes/Ui/OverheadUi.tscn");
    
    private OverheadUiNode() { }
    
    public static OverheadUiNode Instance() => Scene.Instance<OverheadUiNode>();
    
    public OverheadUi? OverheadUi { get; set; }

    public override void _Process(float delta) => OverheadUi?.Process(delta);
}
