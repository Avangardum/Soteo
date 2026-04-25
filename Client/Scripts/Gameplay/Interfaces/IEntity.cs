namespace Soteo.Gameplay.Interfaces;

public interface IEntity
{
    event Action Removed;
    
    Guid Id { get; }
    Vector2 Position { get; set; }
    float Azimuth { get; set; }
    Node2D Node { get; }
    
    EntitySnapshot CreateSnapshot();
    void ReplicateSnapshot(EntitySnapshot snapshot);
    void Remove();
}