namespace Soteo.Core.StaticHelpers;

public static class Const
{
    public const string Version = "0.1";
    public const int BytesInGuid = 16;
    public static readonly Guid CampaignServerId = Guid.Parse("00000000-0000-0000-0000-00000000b055");
    public static readonly Guid SingleplayerPlayerId = Guid.Parse("00000000-0000-0000-0000-000000005010");
    
    public const int TicksPerSecond = 20;
    public const double TickInterval = 1.0 / TicksPerSecond;
}
