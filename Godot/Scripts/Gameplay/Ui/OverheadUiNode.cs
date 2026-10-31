namespace Soteo.Main.Gameplay.Ui;

public sealed partial class OverheadUiNode : Control
{
    private static readonly PackedScene Scene = ResourceLoader.Load<PackedScene>("res://Scenes/Ui/OverheadUi.tscn");

    private OverheadUiNode() { }

    public static OverheadUiNode Instantiate() => Scene.Instantiate<OverheadUiNode>();

    public OverheadUi? OverheadUi { get; set; }

    public override void _Process(double delta) => OverheadUi?.Process(delta);
}
