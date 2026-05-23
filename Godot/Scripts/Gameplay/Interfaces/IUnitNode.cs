namespace Soteo.Gameplay.Interfaces;

public interface IUnitNode : IEntityNode
{
    void MoveAndCollide(GdVector2 movement);
}