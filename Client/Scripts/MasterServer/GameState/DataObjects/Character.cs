namespace Soteo.MasterServer.GameState.DataObjects;

public sealed class Character
{
    public required Guid Id { get; set; }
    public Guid ShardId { get; set; }
}