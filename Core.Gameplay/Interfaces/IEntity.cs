using System.Numerics;

namespace Soteo.Core.Gameplay.Interfaces;

public interface IEntity
{
    event Action Removed;
    
    Guid Id { get; }
    Vector2 Position { get; set; }
    double Azimuth { get; set; }
    
    void Remove();
}
