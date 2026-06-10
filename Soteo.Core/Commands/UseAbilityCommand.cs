using System.Numerics;
using Soteo.Core.Gameplay.Enums;
using Soteo.Core.Gameplay.Interfaces;

namespace Soteo.Core.Gameplay.Commands;

public sealed record UseAbilityCommand
(
    AbilitySlot Slot,
    bool Repeat = false,
    Vector2? TargetPosition = null,
    Guid? TargetUnitId = null,
    Vector2? TargetDirection = null,
    Guid? TargetShardId = null
) : ICommand;