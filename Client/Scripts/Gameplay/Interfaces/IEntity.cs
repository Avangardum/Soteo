namespace Soteo.Gameplay.Interfaces;

public interface IEntity
{
    Guid Id { get; set; }
    Vector2 Position { get; set; }
    float Azimuth { get; set; }
    Node2D Node { get; }
    
    EntitySnapshot CreateSnapshot();
    void ReplicateSnapshot(EntitySnapshot snapshot);
}