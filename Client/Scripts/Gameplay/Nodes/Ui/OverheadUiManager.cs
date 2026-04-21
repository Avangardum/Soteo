using Soteo.Gameplay.Interfaces;
using Soteo.Gameplay.Nodes.Entities;
using Soteo.Shared.Attributes;

namespace Soteo.Gameplay.Nodes.Ui;

public sealed class OverheadUiManager : Control
{
    private ICamera _camera = null!;
    private IPalette _palette = null!;
    private readonly PackedScene _overheadUiScene = ResourceLoader.Load<PackedScene>("res://Scenes/Ui/OverheadUi.tscn");

    public override void _Ready()
    {
        if (IsServer) QueueFree();
    }

    [Inject]
    public void Inject(ICamera camera, IEntityManager entityManager, IPalette palette)
    {
        _camera = camera;
        _palette = palette;
        
        entityManager.EntityAdded += OnEntityAdded;
    }
    
    private void OnEntityAdded(IEntity entity)
    {
        if (entity is not Unit unit) return;
        
        var overheadUi = _overheadUiScene.Instance<OverheadUi>();
        AddChild(overheadUi);
        overheadUi.Inject(unit, _camera, _palette);
    }
}