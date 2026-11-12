using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

namespace Soteo.Core.Statuses;

public sealed record StatusContext : IServiceProvider, ISourceUnitAndAbility
{
    public required Guid Id { get; init; }
    public required Status Status { get; init; }
    public required AbilityContext? SourceAbilityContext { get; init; }
    public required Unit Unit { get; init; }
    public required Unit? SourceUnit { get; init; }
    public required double ElapsedTime { get; init; }
    public required double DisplayElapsedTime { get; init; }
    public required double RemainingTime { get; init; }
    public required long Ordinal { get; init; }
    public required IServiceProvider ServiceProvider { get; init; }

    Unit? ISourceUnitAndAbility.Unit => SourceUnit;
    Ability? ISourceUnitAndAbility.Ability => SourceAbilityContext?.Ability;
    AbilityContext? ISourceUnitAndAbility.AbilityContext => SourceAbilityContext;

    public object? GetService(Type type) => ServiceProvider.GetService(type);

    [MemberNotNull(nameof(SourceAbilityContext))]
    public T SourceAbilityAs<T>() where T : Ability => (T)SourceAbilityContext.Required.Ability;

    public StatusContextSnapshot ToSnapshot()
    {
        return new StatusContextSnapshot
        {
            Id = Id,
            Status = Status,
            AbilityContext = SourceAbilityContext?.ToSnapshot(),
            UnitId = Unit.Id,
            SourceId = SourceUnit?.Id,
            ElapsedTime = ElapsedTime,
            DisplayElapsedTime = DisplayElapsedTime,
            RemainingTime = RemainingTime,
            Ordinal = Ordinal
        };
    }

    public static StatusContext FromSnapshot(StatusContextSnapshot snapshot, IServiceProvider serviceProvider)
    {
        var entityManager = serviceProvider.GetRequiredService<IEntityManager>();
        return new StatusContext
        {
            Id = snapshot.Id,
            Status = snapshot.Status,
            SourceAbilityContext =
                snapshot.AbilityContext?.PassTo(it => AbilityContext.FromSnapshot(it, serviceProvider)),
            Unit = entityManager.GetEntity<Unit>(snapshot.UnitId).Required,
            SourceUnit = snapshot.SourceId?.PassTo(it => entityManager.GetEntity<Unit>(it).Required),
            ElapsedTime = snapshot.ElapsedTime,
            DisplayElapsedTime = snapshot.DisplayElapsedTime,
            RemainingTime = snapshot.RemainingTime,
            Ordinal = snapshot.Ordinal,
            ServiceProvider = serviceProvider
        };
    }
}
