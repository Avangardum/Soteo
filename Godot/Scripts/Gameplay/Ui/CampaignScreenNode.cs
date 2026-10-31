namespace Soteo.Main.Gameplay.Ui;

public sealed partial class CampaignScreenNode : Control
{
    private static readonly PackedScene Scene =
        ResourceLoader.Load<PackedScene>("res://Scenes/Ui/CampaignScreen.tscn");

    private CampaignScreenNode() { }

    public static CampaignScreenNode Instantiate() => Scene.Instantiate<CampaignScreenNode>();
}
