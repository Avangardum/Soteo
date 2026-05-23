using Soteo.Gameplay.Abilities;
using Soteo.Gameplay.Dto;
using Soteo.Gameplay.Dto.Snapshots;
using Soteo.Gameplay.Interfaces;
using Soteo.Shared.Enums;

namespace Soteo.Gameplay.Entities;

public sealed class PlayerCharacter : Unit
{
    public PlayerCharacter(Guid id, IUnitNode node, IServiceProvider serviceProvider) : base(id, node, serviceProvider)
    {
        SetAbility<BloodSacrificeAbility>(AbilitySlot.Class0, 1);
        SetAbility<HealAbility>(AbilitySlot.Class1, 1);
        SetAbility<ProjectileBurstAbility>(AbilitySlot.Class2, 1);
        SetAbility<VampireAbility>(AbilitySlot.Class3, 1);
        SetAbility<RecallAbility>(AbilitySlot.Recall, 1);
        SetAbility<RangedAttackAbility>(AbilitySlot.Attack, 1);
    }
}