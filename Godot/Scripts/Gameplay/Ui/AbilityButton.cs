using Soteo.Util;

namespace Soteo.Main.Gameplay.Ui;

public sealed partial class AbilityButton : TextureButton
{
    private readonly LateInit<TextureRect> _iconRect = new();
    private readonly LateInit<TextureProgressBar> _cooldownIndicator = new();
    private readonly LateInit<TextureProgressBar> _useProgressIndicator = new();
    private readonly LateInit<Label> _healthCostLabel = new();
    private readonly LateInit<Label> _manaCostLabel = new();

    public TextureRect IconRect => _iconRect;
    public TextureProgressBar CooldownIndicator => _cooldownIndicator;
    public TextureProgressBar UseProgressIndicator => _useProgressIndicator;
    public Label HealthCostLabel => _healthCostLabel;
    public Label ManaCostLabel => _manaCostLabel;

    public override void _Ready()
    {
        _iconRect.Value = GetNode<TextureRect>("Icon");
        _cooldownIndicator.Value = GetNode<TextureProgressBar>("Cooldown");
        _useProgressIndicator.Value = GetNode<TextureProgressBar>("UseProgress");
        _healthCostLabel.Value = GetNode<Label>("HealthCost");
        _manaCostLabel.Value = GetNode<Label>("ManaCost");
    }
}
