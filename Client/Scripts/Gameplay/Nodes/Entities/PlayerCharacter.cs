using Soteo.Gameplay.Abilities;
using Soteo.Shared.Enums;

namespace Soteo.Gameplay.Nodes.Entities;

public class PlayerCharacter : Unit
{
    public string DisplayName { get; set; } = "";

    public override void _Ready()
    {
        base._Ready();
        AbilityStatesInternal[AbilitySlot.Class0] = AbilityState.New<BloodSacrificeAbility>(1);
        AbilityStatesInternal[AbilitySlot.Class1] = AbilityState.New<HealAbility>(1);
        AbilityStatesInternal[AbilitySlot.Recall] = AbilityState.New<RecallAbility>(1);
        AbilityStatesInternal[AbilitySlot.Attack] = AbilityState.New<RangedAttack>(1);
    }
}