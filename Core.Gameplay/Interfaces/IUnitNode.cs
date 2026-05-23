using System.Numerics;

namespace Soteo.Gameplay.Interfaces;

public interface IUnitNode : IEntityNode
{
    void MoveAndCollide(Vector2 movement);
}