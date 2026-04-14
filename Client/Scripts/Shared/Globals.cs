using Soteo.Gameplay.Nodes;

namespace Soteo.Shared;

public static class Globals
{
    public static readonly bool IsServer = OS.GetCmdlineArgs().Contains("--server") || Main.EditorIsServer;
    public static readonly bool IsSingleplayer = OS.GetCmdlineArgs().Contains("--singleplayer");
    public static readonly bool IsWeb = OS.HasFeature("web");
    public static readonly bool UseJsmq = IsSingleplayer && IsWeb;
    public static readonly Guid MasterServerId = Guid.Parse("00000000-0000-0000-0000-00000000b055");
}