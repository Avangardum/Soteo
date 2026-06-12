using System.Numerics;
using Microsoft.Extensions.DependencyInjection;
using Soteo.Core.Abilities;
using Soteo.Core.Entities;
using Soteo.Core.Enums;
using Soteo.Core.Interfaces;

namespace Soteo.Core.Dto;

public sealed record AbilityContextSnapshot
{
    public required Ability Ability { get; init; }
    public required int Level { get; init; }
    public required Guid UserId { get; init; }
    public required IReadOnlyDictionary<Stat, double> UserStats { get; init; }
    public Vector2? TargetPosition { get; init; }
    public Guid? TargetUnitId { get; init; }
    public Vector2? TargetDirection { get; init; }
    public Guid? TargetShardId { get; init; }
}
