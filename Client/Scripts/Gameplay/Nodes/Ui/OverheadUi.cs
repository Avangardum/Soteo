using Soteo.Gameplay.Nodes.Entities;
using Soteo.Shared.Extensions;

namespace Soteo.Gameplay.Nodes.Ui;

public sealed class OverheadUi : Control
{
    private static readonly Vector2 Offset = new(0, -18);
    
    private Unit _unit = null!;
    private Camera2D _camera = null!;
    
    private Label _nameLabel = null!;
    private Label _levelLabel = null!;
    private TextureProgress _healthBar = null!;
    private TextureProgress _manaBar = null!;
    
    public void Inject(Unit unit, Camera2D camera)
    {
        _unit = unit;
        _camera = camera;
        
        unit.Connect("tree_exited", this, nameof(OnUnitRemoved));
        
        if (unit is PlayerCharacter playerCharacter)
        {
            _nameLabel.Text = playerCharacter.DisplayName;
        }
    }

    public override void _Ready()
    {
        ProcessPriority = (int)ProcessPriorityEnum.OverheadUi;
        
        _nameLabel = GetNode<Label>("Panel/VBoxContainer/HBoxContainer/Name");
        _levelLabel = GetNode<Label>("Panel/VBoxContainer/HBoxContainer/Level");
        _healthBar = GetNode<TextureProgress>("Panel/VBoxContainer/Health");
        _manaBar = GetNode<TextureProgress>("Panel/VBoxContainer/Mana");
    }

    public override void _Process(float delta)
    {
        RectPosition = (_unit.Position - _camera.GetCameraPosition() + Offset) * _camera.TrueZoom;
    }
    
    public void OnUnitRemoved()
    {
        SetProcess(false);
        QueueFree();
    }
}