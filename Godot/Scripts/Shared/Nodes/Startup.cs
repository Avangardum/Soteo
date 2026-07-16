using Soteo.Core.Enums;
using Soteo.Main.Gameplay;

namespace Soteo.Main.Shared.Nodes;

public sealed class Startup : Node
{
    public override void _Ready()
    {
        if (SharedCmdLineArgs.Side == Side.CampaignServer)
            GetTree().ChangeScene("res://Scenes/CampaignServer.tscn");
        else
            GetTree().ChangeScene("res://Scenes/Main.tscn");
    }
}
