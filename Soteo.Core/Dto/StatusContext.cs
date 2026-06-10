using System.Diagnostics.CodeAnalysis;
using Soteo.Core.Gameplay.Abilities;
using Soteo.Core.Gameplay.Entities;
using Soteo.Core.Gameplay.Interfaces;
using Soteo.Core.Gameplay.Statuses;

namespace Soteo.Core.Gameplay.Dto;

public sealed record StatusContext : IServiceProvider, ISourceUnitAndAbility
{
    public required Guid Id { get; init; }
    public required Status Status { get; init; }
    public required AbilityContext? SourceAbilityContext { get; init; }
    public required IUnit Unit { get; init; }
    public required IUnit? SourceUnit { get; init; }
    public required StatusTickContext? Tick { get; init; }
    public required double ElapsedTime { get; init; }
    public required double DisplayElapsedTime { get; init; }
    public required double RemainingTime { get; init; }
    public required long Ordinal { get; init; }
    public required IServiceProvider ServiceProvider { get; init; }
    
    IUnit? ISourceUnitAndAbility.Unit => SourceUnit;
    Ability? ISourceUnitAndAbility.Ability => SourceAbilityContext?.Ability;
    AbilityContext? ISourceUnitAndAbility.AbilityContext => SourceAbilityContext;
    
    public object? GetService(Type type) => ServiceProvider.GetService(type);
    
    [MemberNotNull(nameof(SourceAbilityContext))]
    public T SourceAbilityAs<T>() where T : Ability => (T)SourceAbilityContext.Required.Ability;
    
    public DeflatedStatusContext Deflate()
    {
        return new DeflatedStatusContext
        {
            Id = Id,
            Status = Status,
            AbilityContext = SourceAbilityContext?.Deflate(),
            UnitId = Unit.Id,
            SourceId = SourceUnit?.Id,
            Tick = Tick,
            ElapsedTime = ElapsedTime,
            DisplayElapsedTime = DisplayElapsedTime,
            RemainingTime = RemainingTime,
            Ordinal = Ordinal
        };
    }
}
