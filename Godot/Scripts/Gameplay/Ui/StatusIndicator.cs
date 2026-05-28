using Soteo.Util;

namespace Soteo.Gameplay.Ui;

public sealed class StatusIndicator : TextureProgress
{
    private readonly LateInit<TextureRect> _iconRect = new();
    
    public TextureRect IconRect => _iconRect;
    
    public override void _Ready()
    {
        _iconRect.Value = GetNode<TextureRect>("Icon");
    }
}
