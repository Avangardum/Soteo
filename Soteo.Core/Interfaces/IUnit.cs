using Soteo.Core.Abilities;
using Soteo.Core.Dto;
using Soteo.Core.Enums;
using Soteo.Core.Statuses;
using Soteo.Util.Interfaces;

namespace Soteo.Core.Interfaces;

public interface IUnit : IEntity
{
    bool IsDead { get; }
    void Die();
    
    IReadOnlySet<Guid> ControllingPlayerIds { get; }
    Faction Faction { get; }
    bool IsAlliedTo(IUnit other);
    
    IReadOnlyDictionary<Stat, double> Stats { get; }
    
    AbilityUseProgress? AbilityUseProgress { get; }
    IReadOnlyDictionary<AbilitySlot, AbilitySlotState> AbilitySlotStates { get; }
    
    IReadOnlyDictionary<Guid, StatusContext> Statuses { get; }
    
    void AddStatus
    (
        Status status,
        double time,
        double? tickInterval,
        IUnit? sourceUnit,
        AbilityContext? sourceAbilityContext
    ); 
    
    void RemoveStatus(Guid id);
    
    void SpendHealth(double amount, Ability? sourceAbility);
    void SpendMana(double amount, Ability? sourceAbility);
    void RestoreHealth(double amount, IUnit? sourceUnit, Ability? sourceAbility);
    void RestoreMana(double amount, IUnit? sourceUnit, Ability? sourceAbility);
    
    void TakeDamage(double amount, IUnit? sourceUnit, Ability? sourceAbility);
    void DealAttackDamageTo(IUnit target, Ability sourceAbility);
}