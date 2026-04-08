namespace Soteo.Gameplay;

public sealed record EntitySnapshot
{
    public required Guid Id { get; init; }
    public Vector2? Position { get; init; }
    public float? Azimuth { get; init; }
}