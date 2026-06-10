using System.Numerics;

namespace Soteo.Core.Interfaces;

public interface IUnitNode : IEntityNode
{
    void MoveAndCollide(Vector2 movement);
}
