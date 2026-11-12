namespace Soteo.Core.Abilities;

public enum AbilityValidationResult
{
    Ok,
    OutOfRange,
    OutOfAngularRange,
    InvalidTarget,
    NotEnoughHealth,
    NotEnoughMana,
}
