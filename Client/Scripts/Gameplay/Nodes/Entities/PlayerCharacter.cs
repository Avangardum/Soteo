using Soteo.Gameplay.Abilities;
using Soteo.Shared.Enums;

namespace Soteo.Gameplay.Nodes.Entities;

public class PlayerCharacter : Unit
{
    public string DisplayName { get; set; } = "";

    public override void _Ready()
    {
        base._Ready();
        AbilityStates[AbilitySlot.Unit0] = AbilityState.New<HealAbility>(1);
        AbilityStates[AbilitySlot.Unit1] = AbilityState.New<BloodSacrificeAbility>(1);
        AbilityStates[AbilitySlot.Recall] = AbilityState.New<RecallAbility>(1);
    }
}