using System.Diagnostics.CodeAnalysis;
using Soteo.Gameplay.Abilities;
using Soteo.Gameplay.Entities;
using Soteo.Gameplay.Statuses;
using Soteo.Shared.Extensions;

namespace Soteo.Gameplay.Dto;

public sealed record StatusContext : IServiceProvider
{
    public required Guid Id { get; init; }
    public required Status Status { get; init; }
    public required AbilityContext? AbilityContext { get; init; }
    public required Unit Unit { get; init; }
    public required Unit? Source { get; init; }
    public required float TickCountdown { get; init; }
    public required float DisplayElapsedTime { get; init; }
    public required float RemainingTime { get; init; }
    public required float TickInterval { get; init; }
    public required IServiceProvider ServiceProvider { get; init; }
    
    public object? GetService(Type type) => ServiceProvider.GetService(type);
    
    [MemberNotNull(nameof(AbilityContext))]
    public T AbilityAs<T>() where T : Ability => (T)AbilityContext.Required.Ability;
}