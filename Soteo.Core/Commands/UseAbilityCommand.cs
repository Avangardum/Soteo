using System.Numerics;
using Soteo.Core.Enums;
using Soteo.Core.Interfaces;

namespace Soteo.Core.Commands;

// todo to property based style
public sealed record UseAbilityCommand
(
    AbilitySlot Slot,
    bool Repeat = false,
    bool Alt = false,
    Vector2? TargetPosition = null,
    Guid? TargetUnitId = null,
    Vector2? TargetDirection = null,
    Guid? TargetShardId = null
) : ICommand;