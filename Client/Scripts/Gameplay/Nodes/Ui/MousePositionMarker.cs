namespace Soteo.Gameplay.Nodes.Ui;

public sealed class MousePositionMarker : Line2D
{
    public override void _Process(float delta)
    {
        Position = GetGlobalMousePosition();
    }
}