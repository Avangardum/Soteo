namespace Soteo.Main.Gameplay.Ui;

public sealed partial class StatusIndicator : TextureProgressBar
{
    public TextureRect IconRect { get; private set; } = null!;

    public override void _Ready()
    {
        IconRect = GetNode<TextureRect>("Icon");
    }
}
