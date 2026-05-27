using System.Numerics;

namespace Soteo.Core.Gameplay.Interfaces;

public interface IEntityNode // todo clean up entity scenes
{
    IEntity? Entity { get; set; }
    Vector2 Position { get; set; }
}
