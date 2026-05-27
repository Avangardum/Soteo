using System.Diagnostics.CodeAnalysis;
using Soteo.Core.Gameplay.Abilities;
using Soteo.Core.Gameplay.Entities;
using Soteo.Core.Gameplay.Statuses;

namespace Soteo.Core.Gameplay.Dto;

public sealed record StatusContext : IServiceProvider
{
    public required Guid Id { get; init; }
    public required Status Status { get; init; }
    public required AbilityContext? AbilityContext { get; init; }
    public required Unit Unit { get; init; }
    public required Unit? Source { get; init; }
    public required StatusTickContext? Tick { get; init; }
    public required double ElapsedTime { get; init; }
    public required double DisplayElapsedTime { get; init; }
    public required double RemainingTime { get; init; }
    public required long Ordinal { get; init; }
    public required IServiceProvider ServiceProvider { get; init; }
    
    public object? GetService(Type type) => ServiceProvider.GetService(type);
    
    [MemberNotNull(nameof(AbilityContext))]
    public T AbilityAs<T>() where T : Ability => (T)AbilityContext.Required.Ability;
    
    public DeflatedStatusContext Deflate()
    {
        return new DeflatedStatusContext
        {
            Id = Id,
            Status = Status,
            AbilityContext = AbilityContext?.Deflate(),
            UnitId = Unit.Id,
            SourceId = Source?.Id,
            Tick = Tick,
            ElapsedTime = ElapsedTime,
            DisplayElapsedTime = DisplayElapsedTime,
            RemainingTime = RemainingTime,
            Ordinal = Ordinal
        };
    }
}
