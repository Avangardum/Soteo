using Microsoft.Extensions.DependencyInjection;
using Soteo.Gameplay.Interfaces;
using Soteo.Gameplay.Nodes.Entities;
using Soteo.Shared.Packets;

namespace Soteo.Gameplay.Abilities;

public sealed class RecallAbility : Ability<RecallAbility>
{
    public override Scalable<float> CastTimeSec => 10;
    
    public override void OnCasted(AbilityCastContext context)
    {
        base.OnCasted(context);
        context.Caster.QueueFree();
        context.GetRequiredService<IPacketSender>()
            .SendReliable(new CharacterRecalledPacket { CharacterId = context.Caster.Id }, MasterServerId);
    }
}