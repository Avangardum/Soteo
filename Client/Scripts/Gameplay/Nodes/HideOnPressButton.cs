namespace Soteo.Gameplay.Nodes;

public sealed class HideOnPressButton : Button
{
    public override void _Pressed() => Visible = false;
}