namespace Soteo.Util;

public static class Const
{
    public const string Version = "0.1";
    public const int BytesInGuid = 16;
    public static readonly Guid CampaignServerId = Guid.Parse("00000000-0000-0000-0000-00000000b055");
    public static readonly LateInit<bool> IsServer = new();
    public static readonly LateInit<bool> IsSingleplayer = new();
    public static readonly LateInit<bool> IsWeb = new();
    
    public static bool UseJsmq => IsSingleplayer && IsWeb;
}
