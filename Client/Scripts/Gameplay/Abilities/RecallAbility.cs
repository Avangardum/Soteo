using Microsoft.Extensions.DependencyInjection;
using Soteo.Gameplay.Interfaces;
using Soteo.Gameplay.Nodes.Entities;
using Soteo.Shared.Packets;

namespace Soteo.Gameplay.Abilities;

public sealed class RecallAbility : UntargetedAbility<RecallAbility>
{
    public override Scalable<float> CastTimeSeconds => 10;
    
    public override void Cast(AbilityCastContext context)
    {
        context.Caster.QueueFree();
        context.GetRequiredService<IPacketSender>()
            .SendReliable(new CharacterRecalledPacket { CharacterId = context.Caster.Id }, MasterServerId);
    }
}