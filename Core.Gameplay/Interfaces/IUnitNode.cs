using System.Numerics;

namespace Soteo.Core.Gameplay.Interfaces;

public interface IUnitNode : IEntityNode
{
    void MoveAndCollide(Vector2 movement);
}