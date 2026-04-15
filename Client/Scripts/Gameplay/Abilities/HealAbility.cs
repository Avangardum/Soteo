using Soteo.Gameplay.Nodes.Entities;

namespace Soteo.Gameplay.Abilities;

public sealed class HealAbility(Unit owner) : UnitTargetedAbility(owner)
{
    public override int MaxLevel => 4;

    public override int ManaCost => CurrentLevel switch
    {
        1 => 100,
        2 => 120,
        3 => 160,
        4 => 180
    };
    
    private int Heal => CurrentLevel switch
    {
        1 => 200,
        2 => 300,
        3 => 400,
        4 => 500
    };

    public override float Cooldown => CurrentLevel switch
    {
        1 => 15,
        2 => 13,
        3 => 11,
        4 => 9
    };

    public override float CastTimeSeconds => 0.5f;

    public override float CastRange => 300;

    public override bool IsValidTarget(Unit target) => target.IsAlliedTo(owner);

    public override void Cast(Unit target) => target.CurrentHealth += Heal;
}