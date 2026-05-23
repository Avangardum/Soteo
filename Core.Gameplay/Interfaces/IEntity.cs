using System.Numerics;
using Soteo.Gameplay.Dto.Snapshots;

namespace Soteo.Gameplay.Interfaces;

public interface IEntity
{
    event Action Removed;
    
    Guid Id { get; }
    Vector2 Position { get; set; }
    double Azimuth { get; set; }
    
    EntitySnapshot CreateSnapshot();
    void ReplicateSnapshot(EntitySnapshot snapshot);
    void ApplyDelta(EntitySnapshotDelta delta, double interpolationWeight);
    void Remove();
}