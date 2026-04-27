using Soteo.Gameplay.Entities;
using Soteo.Gameplay.Interfaces;

namespace Soteo.Gameplay.Ui;

public sealed class OverheadUiManager : Control
{
    private readonly ICamera _camera;
    private readonly IPalette _palette;
    private readonly PackedScene _overheadUiScene = ResourceLoader.Load<PackedScene>("res://Scenes/Ui/OverheadUi.tscn");
    
    public OverheadUiManager(ICamera camera, IEntityManager entityManager, IPalette palette)
    {
        Name = nameof(OverheadUiManager);
        AnchorLeft = AnchorRight = AnchorTop = AnchorBottom = 0.5f;
        
        _camera = camera;
        _palette = palette;
        
        entityManager.EntityAdded += OnEntityAdded;
    }
    
    private void OnEntityAdded(IEntity entity)
    {
        if (entity is not Unit unit) return;
        
        AddChild(new OverheadUi(unit, _camera, _palette));
    }
}