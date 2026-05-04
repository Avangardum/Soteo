using Soteo.Gameplay.EntityNodes;

namespace Soteo.Gameplay.Interfaces;

public interface IEntityNodePool
{
    UnitNode GetUnitNode();
    ProjectileNode GetProjectileNode();
    void ReturnNode(IEntityNode node);
}