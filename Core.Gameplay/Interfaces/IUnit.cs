using Soteo.Core.Gameplay.Abilities;
using Soteo.Core.Gameplay.Dto;
using Soteo.Core.Gameplay.Enums;
using Soteo.Core.Gameplay.Interfaces;
using Soteo.Core.Gameplay.Statuses;
using Soteo.Util.Interfaces;

namespace Soteo.Core.Gameplay.Interfaces
{
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
}

namespace Soteo.Core.Gameplay.Extensions
{
    public static class UnitExtensions
    {
        extension (IUnit self)
        {
            public void AddStatus(Status status, double time, double? tickInterval, ISourceUnitAndAbility? source)
            {
                self.AddStatus(status, time, tickInterval, source?.Unit, source?.AbilityContext);
            }

            public void AddStatus<T>
            (
                double time,
                double? tickInterval,
                IUnit? sourceUnit,
                AbilityContext? sourceAbilityContext
            ) where T : Status, new()
            {
                self.AddStatus(Status.Instance<T>(), time, tickInterval, sourceUnit, sourceAbilityContext);
            }

            public void AddStatus<T>(double time, double? tickInterval, ISourceUnitAndAbility? source)
                where T : Status, new()
            {
                self.AddStatus<T>(time, tickInterval, source?.Unit, source?.AbilityContext);
            }

            public void RestoreHealth(double amount, ISourceUnitAndAbility? source) =>
                self.RestoreHealth(amount, source?.Unit, source?.Ability);
        
            public void RestoreMana(double amount, ISourceUnitAndAbility? source) =>
                self.RestoreMana(amount, source?.Unit, source?.Ability);
        
            public void TakeDamage(double amount, ISourceUnitAndAbility? source) =>
                self.TakeDamage(amount, source?.Unit, source?.Ability);
        }
    }
}
