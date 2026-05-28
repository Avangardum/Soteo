using Soteo.Util;

namespace Soteo.Gameplay.Ui;

public sealed class AbilityButton : TextureButton
{
    private readonly LateInit<TextureRect> _iconRect = new();
    private readonly LateInit<TextureProgress> _cooldownIndicator = new();
    private readonly LateInit<TextureProgress> _useProgressIndicator = new();
    private readonly LateInit<Label> _healthCostLabel = new();
    private readonly LateInit<Label> _manaCostLabel = new();
    
    public TextureRect IconRect => _iconRect;
    public TextureProgress CooldownIndicator => _cooldownIndicator;
    public TextureProgress UseProgressIndicator => _useProgressIndicator;
    public Label HealthCostLabel => _healthCostLabel;
    public Label ManaCostLabel => _manaCostLabel;

    public override void _Ready()
    {
        _iconRect.Value = GetNode<TextureRect>("Icon");
        _cooldownIndicator.Value = GetNode<TextureProgress>("Cooldown");
        _useProgressIndicator.Value = GetNode<TextureProgress>("UseProgress");
        _healthCostLabel.Value = GetNode<Label>("HealthCost");
        _manaCostLabel.Value = GetNode<Label>("ManaCost");
    }
}
