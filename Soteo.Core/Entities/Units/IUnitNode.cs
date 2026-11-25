namespace Soteo.Core.Entities.Units;

public interface IUnitNode : IEntityNode
{
    void Move(Vector2 movement);
    Vector2 NextPathPosition(Vector2 target);
}
