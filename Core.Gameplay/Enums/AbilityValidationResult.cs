namespace Soteo.Core.Gameplay.Enums;

public enum AbilityValidationResult
{
    Ok,
    OutOfRange,
    OutOfAngularRange,
    InvalidTarget,
    NotEnoughHealth,
    NotEnoughMana,
    InvalidLevel
}