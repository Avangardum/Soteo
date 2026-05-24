using Soteo.Core.Gameplay.Abilities;
using Soteo.Core.Gameplay.Dto;
using Soteo.Core.Gameplay.Enums;
using Soteo.Core.Gameplay.Statuses;
using static Soteo.Core.Shared.SerializationHelper;

namespace Soteo.Core.Gameplay;

public static class GameplaySerializationHelper
{
    public static void SerializeAbility(Ability value, Stream stream) => SerializeInt(value.Id, stream);
    
    public static Ability DeserializeAbility(Stream stream) => Ability.All[DeserializeInt(stream)];
    
    public static void SerializeStatus(Status value, Stream stream) => SerializeInt(value.Id, stream);
    
    public static Status DeserializeStatus(Stream stream) => Status.All[DeserializeInt(stream)];
    
    public static void SerializePuppetStatusContext(PuppetStatusContext value, Stream stream)
    {
        SerializeGuid(value.Id, stream);
        SerializeStatus(value.Status, stream);
        SerializeNullableClass(value.Ability, SerializeAbility, stream);
        SerializeDouble(value.ElapsedTime, stream);
        SerializeDouble(value.DisplayElapsedTime, stream);
        SerializeDouble(value.RemainingTime, stream);
        SerializeLong(value.Ordinal, stream);
    }
    
    public static PuppetStatusContext DeserializePuppetStatusContext(Stream stream)
    {
        return new PuppetStatusContext
        {
            Id = DeserializeGuid(stream),
            Status = DeserializeStatus(stream),
            Ability = DeserializeNullableClass(DeserializeAbility, stream),
            ElapsedTime = DeserializeDouble(stream),
            DisplayElapsedTime = DeserializeDouble(stream),
            RemainingTime = DeserializeDouble(stream),
            Ordinal = DeserializeLong(stream)
        };
    }
    
    public static void SerializeAbilityUseProgress(AbilityUseProgress value, Stream stream)
    {
        SerializeEnum(value.Slot, stream);
        SerializeDouble(value.ElapsedTime, stream);
        SerializeDouble(value.RemainingTime, stream);
    }
    
    public static AbilityUseProgress DeserializeAbilityUseProgress(Stream stream)
    {
        return new AbilityUseProgress
        {
            Slot = DeserializeEnum<AbilitySlot>(stream),
            ElapsedTime = DeserializeDouble(stream),
            RemainingTime = DeserializeDouble(stream)
        };
    }
    
    public static void SerializeAbilitySlotState(AbilitySlotState value, Stream stream)
    {
        SerializeInt(value.Ability.Id, stream);
        SerializeInt(value.Level, stream);
        SerializeDouble(value.Cooldown, stream);
        SerializeDouble(value.MaxCooldown, stream);
    }
    
    public static AbilitySlotState DeserializeAbilitySlotState(Stream stream)
    {
        return new AbilitySlotState
        {
            Ability = Ability.All[DeserializeInt(stream)],
            Level = DeserializeInt(stream),
            Cooldown = DeserializeDouble(stream),
            MaxCooldown = DeserializeDouble(stream)
        };
    }
}
