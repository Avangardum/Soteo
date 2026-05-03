using Soteo.Gameplay.Abilities;
using Soteo.Gameplay.Dto;
using Soteo.Gameplay.Dto.Snapshots;
using Soteo.Shared.Enums;

namespace Soteo.Gameplay.Entities;

public sealed class PlayerCharacter : Unit
{
    private static readonly PackedScene Scene = ResourceLoader.Load<PackedScene>("res://Scenes/Player.tscn");
    
    public PlayerCharacter(Guid id, IServiceProvider serviceProvider) : base(id, Scene, serviceProvider)
    {
        DisplayName = id.ToString()[^12..];
        
        SetAbility<BloodSacrificeAbility>(AbilitySlot.Class0, 1);
        SetAbility<HealAbility>(AbilitySlot.Class1, 1);
        SetAbility<ProjectileBurstAbility>(AbilitySlot.Class2, 1);
        SetAbility<VampireAbility>(AbilitySlot.Class3, 1);
        SetAbility<RecallAbility>(AbilitySlot.Recall, 1);
        SetAbility<RangedAttackAbility>(AbilitySlot.Attack, 1);
    }
    
    public PlayerCharacter(UnitSnapshot snapshot, IServiceProvider serviceProvider) :
        this(snapshot.Id, serviceProvider) { }
    
    public string DisplayName { get; }
}