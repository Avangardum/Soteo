using Soteo.Gameplay.Abilities;
using Soteo.Shared.Enums;

namespace Soteo.Gameplay.Nodes.Entities;

public sealed class PlayerCharacter : Unit
{
    private static readonly PackedScene Scene = ResourceLoader.Load<PackedScene>("res://Scenes/Player.tscn");
    
    public PlayerCharacter(Guid id, IServiceProvider serviceProvider) : base(id, Scene, serviceProvider)
    {
        DisplayName = id.ToString()[^12..];
        
        AbilityStatesInternal[AbilitySlot.Class0] = AbilityState.New<BloodSacrificeAbility>(1);
        AbilityStatesInternal[AbilitySlot.Class1] = AbilityState.New<HealAbility>(1);
        AbilityStatesInternal[AbilitySlot.Recall] = AbilityState.New<RecallAbility>(1);
        AbilityStatesInternal[AbilitySlot.Attack] = AbilityState.New<RangedAttack>(1);
    }
    
    public PlayerCharacter(UnitSnapshot snapshot, IServiceProvider serviceProvider) :
        this(snapshot.Id, serviceProvider) { }
    
    public string DisplayName { get; }
}