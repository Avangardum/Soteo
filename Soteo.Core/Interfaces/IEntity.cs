using System.Numerics;

namespace Soteo.Core.Interfaces;

public interface IEntity
{
    event Action Removed;
    
    Guid Id { get; }
    bool IsRemoved { get; }
    Vector2 Position { get; set; }
    double Azimuth { get; set; }
    
    void Remove();
}
