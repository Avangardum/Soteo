using Soteo.Gameplay.Entities;
using Soteo.Gameplay.Enums;
using Soteo.Gameplay.Interfaces;
using Soteo.Shared.Enums;

namespace Soteo.Gameplay.Ui;

public sealed class OverheadUi : Control
{
    private enum Variant
    {
        None,
        PlayerCharacter,
        TinyHealth
    }
    
    private static readonly PackedScene Scene = ResourceLoader.Load<PackedScene>("res://Scenes/Ui/OverheadUi.tscn");

    private readonly UnitPuppet _unit;
    private readonly ICamera _camera;
    private readonly IPalette _palette;
    
    private readonly Control _playerCharacterPanel;
    private readonly Label _playerCharacterNameLabel;
    private readonly Label _playerCharacterLevelLabel;
    private readonly TextureProgress _playerCharacterHealthBar;
    private readonly TextureProgress _playerCharacterManaBar;
    private readonly Control _tinyHealthPanel;
    private readonly TextureProgress _tinyHealthBar;
    
    private GdVector2 _offset;
    
    public OverheadUi(UnitPuppet unit, ICamera camera, IPalette palette)
    {
        Name = $"{nameof(OverheadUi)} {unit.Id}";
        ProcessPriority = (int)ProcessPriorityEnum.OverheadUi;
        MouseFilter = MouseFilterEnum.Ignore;
        
        _unit = unit;
        _camera = camera;
        _palette = palette;
        
        Scene.InstanceAndReparentTo(this);
        
        _playerCharacterPanel = GetNode<Control>("PlayerCharacter");
        _playerCharacterNameLabel = GetNode<Label>("PlayerCharacter/MarginContainer/VBoxContainer/HBoxContainer/Name");
        _playerCharacterLevelLabel = GetNode<Label>("PlayerCharacter/MarginContainer/VBoxContainer/HBoxContainer/Level");
        _playerCharacterHealthBar = GetNode<TextureProgress>("PlayerCharacter/MarginContainer/VBoxContainer/Health");
        _playerCharacterManaBar = GetNode<TextureProgress>("PlayerCharacter/MarginContainer/VBoxContainer/Mana");
        _tinyHealthPanel = GetNode<Control>("TinyHealth");
        _tinyHealthBar = GetNode<TextureProgress>("TinyHealth/MarginContainer/Health");
        
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
                    _offset = new GdVector2(0, -22);
                    break;
                case Variant.TinyHealth:
                    _playerCharacterPanel.Visible = false;
                    _tinyHealthPanel.Visible = true;
                    _offset = new GdVector2(0, -22);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(value), value, null);
            }
        }
    }

    public override void _Process(float delta)
    {
        RectPosition = (_unit.Position - _camera.Position + _offset) * _camera.TrueZoom;
        SelectVariant();
        SetFaction(_unit.Faction);
        SetHealth((float)_unit.Stats[Stat.CurrentHealth], (float)_unit.Stats[Stat.MaxHealth]);
        SetMana((float)_unit.Stats[Stat.CurrentMana], (float)_unit.Stats[Stat.MaxMana]);
    }
    
    private void SelectVariant()
    {
        const double tinyHealthMinZoom = 0.9;
        const double tinyHealthMaxZoom = 2.4;
        double zoom = _camera.TrueZoom;
        CurrentVariant = zoom < tinyHealthMinZoom ? Variant.None :
            zoom <= tinyHealthMaxZoom ? Variant.TinyHealth :
            Variant.PlayerCharacter;
    }
    
    private void SetFaction(Faction faction)
    {
        Color color = faction switch
        {
            Faction.Empire => _palette.Empire,
            Faction.Syndicate => _palette.Syndicate,
            Faction.Neutral => _palette.Neutral
        };
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
    
    public void OnUnitRemoved()
    {
        SetProcess(false);
        QueueFree();
    }
}