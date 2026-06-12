using Microsoft.Extensions.DependencyInjection;
using Soteo.Core.Entities;
using Soteo.Core.Interfaces;
using Soteo.Core.Statuses;

namespace Soteo.Core.Dto;

public sealed record StatusContextSnapshot
{
    public required Guid Id { get; init; }
    public required Status Status { get; init; }
    public required AbilityContextSnapshot? AbilityContext { get; init; }
    public required Guid UnitId { get; init; }
    public required Guid? SourceId { get; init; }
    public required StatusTickContext? Tick { get; init; }
    public required double ElapsedTime { get; init; }
    public required double DisplayElapsedTime { get; init; }
    public required double RemainingTime { get; init; }
    public required long Ordinal { get; init; }
    
    public PuppetStatusContext ToPuppet()
    {
        return new PuppetStatusContext
        {
            Id = Id,
            Status = Status,
            Ability = AbilityContext?.Ability,
            DisplayElapsedTime = DisplayElapsedTime,
            RemainingTime = RemainingTime,
            Ordinal = Ordinal
        };
    }
}
