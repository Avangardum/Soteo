namespace Soteo.Core.Entities.Units;

public interface IUnitNode : IEntityNode
{
    void MoveAndCollide(Vector2 movement);
}
