namespace Soteo.Gameplay.Enums;

[Flags]
public enum AbilityTargetFlags : byte
{
    None = 0,
    Untargeted = 1,
    Point = 1 << 1,
    Unit = 1 << 2,
    HasDirection = 1 << 3,
    HasShard = 1 << 4
}