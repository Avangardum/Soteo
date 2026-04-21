namespace Soteo.Gameplay.Nodes.Ui;

public sealed class HideOnPressButton : Button
{
    public override void _Pressed() => Visible = false;
}