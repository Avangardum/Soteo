using Soteo.Gameplay;
using Soteo.Util;

namespace Soteo.Shared;

public static class MainConst
{
    public static readonly Guid SingleplayerPlayerId = Guid.Parse("00000000-0000-0000-0000-000000005010");
    public static readonly Guid TestShardId = Guid.Parse("00000000-0000-0000-0000-000000007e57");
    public static readonly int TicksPerSecond = (int)ProjectSettings.GetSetting("physics/common/physics_fps");
    public static readonly Texture PlaceholderIcon =
        ResourceLoader.Load<Texture>("res://Textures/Icons/Placeholder.png");

    public static void InitConst()
    {
        Const.IsServer.Value = OS.GetCmdlineArgs().Contains("--server") || Main.EditorIsServer;
        Const.IsSingleplayer.Value = OS.GetCmdlineArgs().Contains("--singleplayer");
        Const.IsWeb.Value = OS.HasFeature("web");
    }
}
