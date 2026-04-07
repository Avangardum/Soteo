namespace Soteo.MasterServer.GameState.DataObjects;

public sealed record User
{
    public required Guid Id { get; set; }
    public required bool IsConnected { get; set; }
    public required bool IsPlayer { get; set; }
    public required bool IsShard { get; set; }
}