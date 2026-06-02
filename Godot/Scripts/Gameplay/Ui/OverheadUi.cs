using Soteo.Core.Gameplay.Entities;
using Soteo.Core.Gameplay.Enums;
using Soteo.Core.Gameplay.Interfaces;
using Soteo.Gameplay.Interfaces;

namespace Soteo.Gameplay.Ui;

public sealed class OverheadUi
{
    private readonly UnitPuppet _unit;
    private readonly ICamera _camera;
    private readonly IPalette _palette;
    
    private readonly OverheadUiNode _node;
    private readonly Control _playerCharacterPanel;
    private readonly Label _playerCharacterNameLabel;
    private readonly Label _playerCharacterLevelLabel;
    private readonly TextureProgress _playerCharacterHealthBar;
    private readonly TextureProgress _playerCharacterManaBar;
    private readonly Control _tinyHealthPanel;
    private readonly TextureProgress _tinyHealthBar;
    
    private Vector2 _offset;
    
    public OverheadUi(OverheadUiNode node, UnitPuppet unit, ICamera camera, IPalette palette)
    {
        _unit = unit;
        _camera = camera;
        _palette = palette;
        
        node.Name = $"{nameof(OverheadUi)} {unit.Id}";
        node.ProcessPriority = (int)ProcessPriorityEnum.OverheadUi;
        node.OverheadUi = this;
        _node = node;
        _playerCharacterPanel = node.GetNode<Control>("PlayerCharacter");
        _playerCharacterNameLabel = node.GetNode<Label>("PlayerCharacter/MarginContainer/VBoxContainer/HBoxContainer/Name");
        _playerCharacterLevelLabel = node.GetNode<Label>("PlayerCharacter/MarginContainer/VBoxContainer/HBoxContainer/Level");
        _playerCharacterHealthBar = node.GetNode<TextureProgress>("PlayerCharacter/MarginContainer/VBoxContainer/Health");
        _playerCharacterManaBar = node.GetNode<TextureProgress>("PlayerCharacter/MarginContainer/VBoxContainer/Mana");
        _tinyHealthPanel = node.GetNode<Control>("TinyHealth");
        _tinyHealthBar = node.GetNode<TextureProgress>("TinyHealth/MarginContainer/Health");
        
        unit.Removed += OnUnitRemoved;
        
        _playerCharacterNameLabel.Text = unit.Id.ToString()[^12..]; 
    }
    
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
                    _tinyHealthPanel.Visible = false;
                    break;
                case Variant.PlayerCharacter:
                    _playerCharacterPanel.Visible = true;
                    _tinyHealthPanel.Visible = false;
                    _offset = new Vector2(0, -22);
                    break;
                case Variant.TinyHealth:
                    _playerCharacterPanel.Visible = false;
                    _tinyHealthPanel.Visible = true;
                    _offset = new Vector2(0, -22);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }
        }
    }

    public void Process(double delta)
    {
        _node.RectPosition = (_unit.Position - _camera.Position + _offset).ToGd() * _camera.Zoom;
        SelectVariant();
        SetFaction(_unit.Faction);
        SetHealth((float)_unit.Stats[Stat.CurrentHealth], (float)_unit.Stats[Stat.MaxHealth]);
        SetMana((float)_unit.Stats[Stat.CurrentMana], (float)_unit.Stats[Stat.MaxMana]);
    }
    
    private void SelectVariant()
    {
        const double tinyHealthMinZoom = 0.9;
        const double tinyHealthMaxZoom = 2.4;
        CurrentVariant = _camera.Zoom < tinyHealthMinZoom ? Variant.None :
            _camera.Zoom <= tinyHealthMaxZoom ? Variant.TinyHealth :
            Variant.PlayerCharacter;
    }
    
    private void SetFaction(Faction faction)
    {
        Color color = _palette.FactionColor(faction);
        switch (CurrentVariant)
        {
            case Variant.PlayerCharacter:
                _playerCharacterHealthBar.TintProgress = color;
                break;
            case Variant.TinyHealth:
                _tinyHealthBar.TintProgress = color;
                break;
        }
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
    
    private void OnUnitRemoved()
    {
        _node.SetProcess(false);
        _node.QueueFree();
        // todo use pooling
    }
    
    private enum Variant
    {
        None,
        PlayerCharacter,
        TinyHealth
    }
}
