using Soteo.Core.Dto;
using Soteo.Core.Interfaces;
using Soteo.Core.Statuses;

namespace Soteo.Core.Extensions;

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