using Soteo.Core.Enums;

namespace Soteo.Main.Shared.Nodes;

public sealed partial class Main : Node
{
    public override void _Ready()
    {
        Callable.From(() =>
        {
            if (Config.Side == Side.CampaignServer)
                GetTree().ChangeSceneToFile("res://Scenes/CampaignServerMain.tscn");
            else
                GetTree().ChangeSceneToFile("res://Scenes/GameplayMain.tscn");
        }).CallDeferred();
    }
}
