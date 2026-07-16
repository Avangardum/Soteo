namespace Soteo.Main.Gameplay;

public static class GameplayCmdLineArgs
{
    public static bool IsServer { get; }
    
    public static bool IsSingleplayer =>
        IsServer ? ShardServerCmdLineArgs.IsSingleplayer : ClientCmdLineArgs.IsSingleplayer;
    
    static GameplayCmdLineArgs()
    {
        IsServer = OS.GetCmdlineArgs().Contains("--server");
    }
}
