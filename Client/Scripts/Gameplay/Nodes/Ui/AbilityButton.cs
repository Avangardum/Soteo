namespace Soteo.Gameplay.Nodes.Ui;

public sealed class AbilityButton : TextureButton
{
    public TextureRect IconRect { get; private set; } = null!;
    public TextureProgress CooldownIndicator { get; private set; } = null!;
    public TextureProgress UseProgressIndicator { get; private set; } = null!;
    public Label HealthCostLabel { get; private set; } = null!;
    public Label ManaCostLabel { get; private set; } = null!;

    public override void _Ready()
    {
        IconRect = GetNode<TextureRect>("Icon");
        CooldownIndicator = GetNode<TextureProgress>("Cooldown");
        UseProgressIndicator = GetNode<TextureProgress>("UseProgress");
        HealthCostLabel = GetNode<Label>("HealthCost");
        ManaCostLabel = GetNode<Label>("ManaCost");
    }
}