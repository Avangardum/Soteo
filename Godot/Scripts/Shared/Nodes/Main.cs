using Soteo.Core.Enums;
using Soteo.Main.Gameplay;

namespace Soteo.Main.Shared.Nodes;

public sealed class Main : Node
{
    public override void _Ready()
    {
        if (Config.Side == Side.CampaignServer)
            GetTree().ChangeScene("res://Scenes/CampaignServerMain.tscn");
        else
            GetTree().ChangeScene("res://Scenes/GameplayMain.tscn");
    }
}
