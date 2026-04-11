using Soteo.Gameplay.Interfaces;
using Soteo.Gameplay.Nodes.Entities;
using Soteo.Shared.Attributes;

namespace Soteo.Gameplay.Nodes.Ui;

public sealed class OverheadUiManager : Control
{
    private Camera2D _camera = null!;
    private readonly PackedScene _overheadUiScene = ResourceLoader.Load<PackedScene>("res://Scenes/Ui/OverheadUi.tscn");

    public override void _Ready()
    {
        if (IsServer) QueueFree();
    }

    [Inject]
    public void Inject(Camera2D camera, IEntityManager entityManager)
    {
        _camera = camera;
        
        entityManager.EntityAdded += OnEntityAdded;
    }
    
    private void OnEntityAdded(IEntity entity)
    {
        if (entity is not Unit unit) return;
        
        var overheadUi = _overheadUiScene.Instance<OverheadUi>();
        AddChild(overheadUi);
        overheadUi.Inject(unit, _camera);
    }
}