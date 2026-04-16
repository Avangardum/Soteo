using Soteo.Gameplay.Nodes.Entities;
using Soteo.Shared.Enums;
using Soteo.Shared.Extensions;

namespace Soteo.Gameplay.Nodes.Ui;

public sealed class OverheadUi : Control
{
    private enum Variant
    {
        None,
        PlayerCharacter,
        TinyHealth
    }

    private Unit _unit = null!;
    private Camera2D _camera = null!;
    
    private Control _playerCharacterPanel = null!;
    private Label _playerCharacterNameLabel = null!;
    private Label _playerCharacterLevelLabel = null!;
    private TextureProgress _playerCharacterHealthBar = null!;
    private TextureProgress _playerCharacterManaBar = null!;
    private TextureProgress _tinyHealthBar = null!;
    
    private Vector2 _offset;
    
    private Variant CurrentVariant
    {
        get;
        set
        {
            if (field == value) return;
            field = value;
            switch (value)
            {
                case Variant.None:
                    _playerCharacterPanel.Visible = false;
                    _tinyHealthBar.Visible = false;
                    break;
                case Variant.PlayerCharacter:
                    _playerCharacterPanel.Visible = true;
                    _tinyHealthBar.Visible = false;
                    _offset = new Vector2(0, -18);
                    break;
                case Variant.TinyHealth:
                    _playerCharacterPanel.Visible = false;
                    _tinyHealthBar.Visible = true;
                    _offset = new Vector2(0, -20);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }
        }
    }
    
    public void Inject(Unit unit, Camera2D camera)
    {
        _unit = unit;
        _camera = camera;
        
        unit.Connect("tree_exited", this, nameof(OnUnitRemoved));
        
        if (unit is PlayerCharacter playerCharacter)
        {
            _playerCharacterNameLabel.Text = playerCharacter.DisplayName;
        }
    }

    public override void _Ready()
    {
        ProcessPriority = (int)ProcessPriorityEnum.OverheadUi;
        
        _playerCharacterPanel = GetNode<Control>("PlayerCharacter");
        _playerCharacterNameLabel = GetNode<Label>("PlayerCharacter/VBoxContainer/HBoxContainer/Name");
        _playerCharacterLevelLabel = GetNode<Label>("PlayerCharacter/VBoxContainer/HBoxContainer/Level");
        _playerCharacterHealthBar = GetNode<TextureProgress>("PlayerCharacter/VBoxContainer/Health");
        _playerCharacterManaBar = GetNode<TextureProgress>("PlayerCharacter/VBoxContainer/Mana");
        _tinyHealthBar = GetNode<TextureProgress>("TinyHealth");
    }

    public override void _Process(float delta)
    {
        RectPosition = (_unit.Position - _camera.GetCameraPosition() + _offset) * _camera.TrueZoom;
        SelectVariant();
        SetHealth(_unit.Stats[Stat.CurrentHealth], _unit.Stats[Stat.MaxHealth]);
        SetMana(_unit.Stats[Stat.CurrentMana], _unit.Stats[Stat.MaxMana]);
    }
    
    private void SelectVariant()
    {
        const float tinyHealthMinZoom = 0.9f;
        const float tinyHealthMaxZoom = 2.4f;
        float zoom = _camera.TrueZoom.x;
        CurrentVariant = zoom < tinyHealthMinZoom ? Variant.None : zoom <= tinyHealthMaxZoom ? Variant.TinyHealth :
            Variant.PlayerCharacter;
    }
    
    private void SetHealth(float current, float max)
    {
        switch (CurrentVariant)
        {
            case Variant.PlayerCharacter:
                _playerCharacterHealthBar.Value = current;
                _playerCharacterHealthBar.MaxValue = max;
                break;
            case Variant.TinyHealth:
                _tinyHealthBar.Value = current;
                _tinyHealthBar.MaxValue = max;
                break;
        }
    }
    
    private void SetMana(float current, float max)
    {
        switch (CurrentVariant)
        {
            case Variant.PlayerCharacter:
                _playerCharacterManaBar.Value = current;
                _playerCharacterManaBar.MaxValue = max;
                break;
        }
    }
    
    public void OnUnitRemoved()
    {
        SetProcess(false);
        QueueFree();
    }
}