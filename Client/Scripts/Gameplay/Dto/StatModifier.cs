using Soteo.Gameplay.Enums;
using Soteo.Shared.Enums;

namespace Soteo.Gameplay.Dto;

public sealed record StatModifier(Stat Stat, StatModifierKind Kind, float Value);