namespace Soteo.Main.Gameplay.Ui;

public sealed partial class AbilityButton : TextureButton
{
    public TextureRect IconRect { get; private set; } = null!;
    public TextureProgressBar CooldownIndicator { get; private set; } = null!;
    public TextureProgressBar UseProgressIndicator { get; private set; } = null!;
    public Label HealthCostLabel { get; private set; } = null!;
    public Label ManaCostLabel { get; private set; } = null!;

    public override void _Ready()
    {
        IconRect = GetNode<TextureRect>("Icon");
        CooldownIndicator = GetNode<TextureProgressBar>("Cooldown");
        UseProgressIndicator = GetNode<TextureProgressBar>("UseProgress");
        HealthCostLabel = GetNode<Label>("HealthCost");
        ManaCostLabel = GetNode<Label>("ManaCost");
    }
}
