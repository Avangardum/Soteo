using Soteo.Gameplay.Interfaces;
using Soteo.Shared.Enums;

namespace Soteo.Gameplay.Commands;

public sealed record UseAbilityCommand
(
    AbilitySlot Slot,
    bool Repeat = false,
    Vector2? TargetPosition = null,
    Guid? TargetUnitId = null,
    Vector2? TargetDirection = null,
    Guid? TargetShardId = null
) : ICommand;