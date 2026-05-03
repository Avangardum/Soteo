namespace Soteo.Gameplay.Enums;

[Flags]
public enum CanTarget
{
    Passive = 0,
    
    // Unprefixed values specify whether that kind of target is allowed
    Nothing = 1 << 0,
    Position = 1 << 1,
    Ally = 1 << 2,
    Enemy = 1 << 3,
    Character = 1 << 4,
    Building = 1 << 5,
    
    // Values prefixed "With" specify whether that data should be included in addition to the target
    WithDirection = 1 << 6,
    WithShard = 1 << 7
        
    // Example: Position | Enemy | Character | WithDirection - Can target either a position or an enemy character and
    // should specify a direction from the target
}