namespace Soteo.Main.Gameplay.Ui;

public sealed partial class PauseScreen : Control
{
    public override void _Process(double delta)
    {
        Visible = GetTree().Paused;
    }
}
