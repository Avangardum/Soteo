using Soteo.Core.Gameplay.Entities;
using Soteo.Core.Gameplay.Interfaces;
using Soteo.Gameplay.Interfaces;

namespace Soteo.Gameplay.Ui;

public sealed class OverheadUiManager : Control
{
    private readonly ICamera _camera;
    private readonly IPalette _palette;
    
    public OverheadUiManager(ICamera camera, IEntityManager entityManager, IPalette palette)
    {
        Name = nameof(OverheadUiManager);
        AnchorLeft = AnchorRight = AnchorTop = AnchorBottom = 0.5f;
        MouseFilter = MouseFilterEnum.Ignore;
        
        _camera = camera;
        _palette = palette;
        
        entityManager.EntityAdded += OnEntityAdded;
    }
    
    private void OnEntityAdded(IEntity entity)
    {
        if (entity is not UnitPuppet unit) return;
        
        var node = OverheadUiNode.Instance();
        AddChild(node);
        new OverheadUi(node, unit, _camera, _palette);
    }
}
