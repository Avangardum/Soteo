using System.Globalization;
using Soteo.Core;

namespace Soteo.Main;

public sealed partial class Main : Node
{
    public override void _Ready()
    {
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

        if ((int)ProjectSettings.GetSetting("physics/common/physics_ticks_per_second") != Const.TicksPerSecond)
            throw new Exception("Godot physics FPS doesn't match Const.TicksPerSecond");

        OS.AddLogger(new CrashOnErrorLogger(GetTree()));

        Callable.From(() =>
        {
            if (Config.Side == Side.CampaignServer)
                GetTree().ChangeSceneToFile("res://Scenes/CampaignServerMain.tscn");
            else
                GetTree().ChangeSceneToFile("res://Scenes/GameplayMain.tscn");
        }).CallDeferred();
    }
}
