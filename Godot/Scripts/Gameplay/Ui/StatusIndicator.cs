namespace Soteo.Gameplay.Ui;

public sealed class StatusIndicator : TextureProgress
{
    public TextureRect IconRect { get; private set; } = null!;
    
    public override void _Ready()
    {
        IconRect = GetNode<TextureRect>("Icon");
    }
}
