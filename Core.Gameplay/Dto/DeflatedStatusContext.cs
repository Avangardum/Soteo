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
    public required double TickCountdown { get; init; }
    public required double DisplayElapsedTime { get; init; }
    public required double RemainingTime { get; init; }
    public required double TickInterval { get; init; }
    public required long Ordinal { get; init; }
    
    public StatusContext Inflate(IServiceProvider serviceProvider)
    {
        var entityManager = serviceProvider.GetRequiredService<IEntityManager>();
        return new StatusContext
        {
            Id = Id,
            Status = Status,
            AbilityContext = AbilityContext?.Inflate(serviceProvider),
            Unit = entityManager.GetEntity<Unit>(UnitId).Required,
            Source = SourceId == null ? null : entityManager.GetEntity<Unit>(SourceId.Value).Required,
            TickCountdown = TickCountdown,
            DisplayElapsedTime = DisplayElapsedTime,
            RemainingTime = RemainingTime,
            TickInterval = TickInterval,
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