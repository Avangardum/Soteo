using Microsoft.Extensions.DependencyInjection;
using Soteo.Core.Gameplay.Entities;
using Soteo.Core.Gameplay.Interfaces;
using Soteo.Core.Gameplay.Statuses;

namespace Soteo.Core.Gameplay.Dto;

public sealed record DeflatedStatusContext
{
    public required Guid Id { get; init; }
    public required Status Status { get; init; }
    public required DeflatedAbilityContext? AbilityContext { get; init; }
    public required Guid UnitId { get; init; }
    public required Guid? SourceId { get; init; }
    public required StatusTickContext? Tick { get; init; }
    public required double ElapsedTime { get; init; }
    public required double DisplayElapsedTime { get; init; }
    public required double RemainingTime { get; init; }
    public required long Ordinal { get; init; }
    
    public StatusContext Inflate(IServiceProvider serviceProvider)
    {
        var entityManager = serviceProvider.GetRequiredService<IEntityManager>();
        return new StatusContext
        {
            Id = Id,
            Status = Status,
            SourceAbilityContext = AbilityContext?.Inflate(serviceProvider),
            Unit = entityManager.GetEntity<Unit>(UnitId).Required,
            SourceUnit = SourceId == null ? null : entityManager.GetEntity<Unit>(SourceId.Value).Required,
            Tick = Tick,
            ElapsedTime = ElapsedTime,
            DisplayElapsedTime = DisplayElapsedTime,
            RemainingTime = RemainingTime,
            Ordinal = Ordinal,
            ServiceProvider = serviceProvider
        };
    }
    
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
