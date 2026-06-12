using Soteo.Core.Abilities;
using Soteo.Core.Enums;
using Soteo.Core.Interfaces;

namespace Soteo.Core.Entities;

public sealed class PlayerCharacterEntity : Unit
{
    public PlayerCharacterEntity
    (
        Guid id,
        Guid controllingPlayerId,
        IUnitNode node,
        IEntityManager entityManager,
        IServiceProvider serviceProvider
    ) : base(id, controllingPlayerId, node, entityManager, serviceProvider)
    {
        SetAbility<BloodSacrificeAbility>(AbilitySlot.Class0, 1);
        SetAbility<HealAbility>(AbilitySlot.Class1, 1);
        SetAbility<ProjectileBurstAbility>(AbilitySlot.Class2, 1);
        SetAbility<VampireAbility>(AbilitySlot.Class3, 1);
        SetAbility<RecallAbility>(AbilitySlot.Recall, 1);
        SetAbility<RangedAttackAbility>(AbilitySlot.Attack, 1);
    }
}
