namespace Soteo.Main.CampaignServer;

public static class EnvironmentVariables
{
    /// <summary>
    /// Path to the Godot executable. Defined by the campaign server and used to start shard server processes.
    /// </summary>
    public static string GodotPath =>
        SysEnvironment.GetRequiredEnvironmentVariable("Soteo__GodotPath");
    
    public static string CampaignSnapshotPath =>
        SysEnvironment.GetRequiredEnvironmentVariable("Soteo__CampaignSnapshotPath");
    
    /// <summary>
    /// Secret used by servers for internal authentication and for token validation. Base64 string.
    /// Defined by all servers.
    /// </summary>
    public static string IntercomSecret =>
        SysEnvironment.GetRequiredEnvironmentVariable("Soteo__IntercomSecret");
}
