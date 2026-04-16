using Soteo.Gameplay.Interfaces;
using Soteo.Gameplay.Nodes.Entities;
using Soteo.Shared.Enums;

namespace Soteo.Gameplay.Nodes.Ui;

public sealed class Hud : Control, IHud
{
    private TextureProgress _healthBar = null!;
    private TextureProgress _manaBar = null!;
    private Label _healthLabel = null!;
    private Label _manaLabel = null!;
    private TextureButton[] _abilityButtons = null!;
    private TextureProgress[] _abilityCooldownIndicators = null!;
    
    public Unit? SelectedUnit { get; set; }

    public override void _Ready()
    {
        if (IsServer)
        {
            SetProcess(false);
            QueueFree();
            return;
        }
        
        _healthBar = GetNode<TextureProgress>("UnitPanel/VBoxContainer/Health");
        _manaBar = GetNode<TextureProgress>("UnitPanel/VBoxContainer/Mana");
        _healthLabel = _healthBar.GetNode<Label>("Label");
        _manaLabel = _manaBar.GetNode<Label>("Label");
        _abilityButtons = GetNode("UnitPanel/VBoxContainer/Abilities").GetChildren().Cast<TextureButton>().ToArray();
        _abilityCooldownIndicators = _abilityButtons.Select(it => it.GetNode<TextureProgress>("Cooldown")).ToArray();
    }

    public override void _Process(float delta)
    {
        if (SelectedUnit == null || !IsInstanceValid(SelectedUnit))
        {
            Visible = false;
            return;
        }

        Visible = true;
        _healthBar.Value = SelectedUnit.Stats[Stat.CurrentHealth];
        _healthBar.MaxValue = SelectedUnit.Stats[Stat.MaxHealth];
        _healthLabel.Text = $"{SelectedUnit.Stats[Stat.CurrentHealth]} / {SelectedUnit.Stats[Stat.MaxHealth]}";
        _manaBar.Value = SelectedUnit.Stats[Stat.CurrentMana];
        _manaBar.MaxValue = SelectedUnit.Stats[Stat.MaxMana];
        _manaLabel.Text = $"{SelectedUnit.Stats[Stat.CurrentMana]} / {SelectedUnit.Stats[Stat.MaxMana]}";
    }
}