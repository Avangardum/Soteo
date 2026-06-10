namespace Soteo.Main.Gameplay.Ui;

public sealed class PauseScreen : Control
{
    public override void _Process(float delta)
    {
        Visible = GetTree().Paused;
    }
}
