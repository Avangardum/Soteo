using System.Collections.Immutable;
using Soteo.Core.Gameplay.Abilities;
using Soteo.Core.Gameplay.Dto;
using Soteo.Core.Gameplay.Enums;
using Soteo.Core.Gameplay.Interfaces;
using Soteo.Core.Gameplay.Statuses;
using Soteo.Core.Shared;
using Soteo.Core.Shared.Interfaces;
using static Soteo.Core.Shared.SerializationHelper;

namespace Soteo.Core.Gameplay;

public class GameplaySerializer(ITypeLocator typeLocator) : IGameplaySerializer
{
    private readonly IReadOnlyList<Type> _abilityTypes = typeLocator.ConcreteSubclassesOf<Ability>();
    private readonly IReadOnlyList<Type> _statusTypes = typeLocator.ConcreteSubclassesOf<Status>();

    public void SerializeAbility(Ability value, Stream stream) =>
        SerializeInt(_abilityTypes.IndexOf(value.GetType()), stream);
    
    public Ability DeserializeAbility(Stream stream) =>
        Ability.Instance(_abilityTypes[DeserializeInt(stream)]);
    
    public void SerializeStatus(Status value, Stream stream) =>
        SerializeInt(_statusTypes.IndexOf(value.GetType()), stream);
    
    public Status DeserializeStatus(Stream stream) =>
        Status.Instance(_statusTypes[DeserializeInt(stream)]);
    
    public void SerializePuppetStatusContext(PuppetStatusContext value, Stream stream)
    {
        SerializeGuid(value.Id, stream);
        SerializeStatus(value.Status, stream);
        SerializeNullableClass(value.Ability, SerializeAbility, stream);
        SerializeDouble(value.DisplayElapsedTime, stream);
        SerializeDouble(value.RemainingTime, stream);
        SerializeLong(value.Ordinal, stream);
    }
    
    public PuppetStatusContext DeserializePuppetStatusContext(Stream stream)
    {
        return new PuppetStatusContext
        {
            Id = DeserializeGuid(stream),
            Status = DeserializeStatus(stream),
            Ability = DeserializeNullableClass(DeserializeAbility, stream),
            DisplayElapsedTime = DeserializeDouble(stream),
            RemainingTime = DeserializeDouble(stream),
            Ordinal = DeserializeLong(stream),
        };
    }
    
    public void SerializeAbilityUseProgress(AbilityUseProgress value, Stream stream)
    {
        SerializeEnum(value.Slot, stream);
        SerializeDouble(value.ElapsedTime, stream);
        SerializeDouble(value.RemainingTime, stream);
    }
    
    public AbilityUseProgress DeserializeAbilityUseProgress(Stream stream)
    {
        return new AbilityUseProgress
        {
            Slot = DeserializeEnum<AbilitySlot>(stream),
            ElapsedTime = DeserializeDouble(stream),
            RemainingTime = DeserializeDouble(stream),
        };
    }
    
    public void SerializeAbilitySlotState(AbilitySlotState value, Stream stream)
    {
        SerializeAbility(value.Ability, stream);
        SerializeInt(value.Level, stream);
        SerializeDouble(value.Cooldown, stream);
        SerializeDouble(value.MaxCooldown, stream);
    }
    
    public AbilitySlotState DeserializeAbilitySlotState(Stream stream)
    {
        return new AbilitySlotState
        {
            Ability = DeserializeAbility(stream),
            Level = DeserializeInt(stream),
            Cooldown = DeserializeDouble(stream),
            MaxCooldown = DeserializeDouble(stream),
        };
    }
}
