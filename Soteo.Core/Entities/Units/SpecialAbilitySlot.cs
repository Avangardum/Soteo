namespace Soteo.Core.Entities.Units;

public sealed class SpecialAbilitySlot : AbilitySlot
{
    private readonly string _name;

    internal SpecialAbilitySlot(string name)
    {
        _name = name;
    }

    public override string ToString() => _name;
}
