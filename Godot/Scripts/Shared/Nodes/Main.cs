using Soteo.Core.Enums;

namespace Soteo.Main.Shared.Nodes;

public sealed class Main : Node
{
    public override void _Ready()
    {
        if (Config.Side == Side.CampaignServer)
            GetTree().ChangeSceneToFile("res://Scenes/CampaignServerMain.tscn");
        else
            GetTree().ChangeSceneToFile("res://Scenes/GameplayMain.tscn");
    }
}
