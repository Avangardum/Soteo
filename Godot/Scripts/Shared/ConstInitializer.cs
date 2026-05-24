using Soteo.Gameplay;
using Soteo.Util;

namespace Soteo.Shared;

public static class ConstInitializer
{
    public static void Init()
    {
        Const.IsServer.Value = OS.GetCmdlineArgs().Contains("--server") || Main.EditorIsServer;
        Const.IsSingleplayer.Value = OS.GetCmdlineArgs().Contains("--singleplayer");
        Const.IsWeb.Value = OS.HasFeature("web");
        Const.TicksPerSecond.Value = (int)ProjectSettings.GetSetting("physics/common/physics_fps");
    }
}
