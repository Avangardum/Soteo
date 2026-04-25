using Microsoft.Extensions.DependencyInjection;
using Soteo.Gameplay.Interfaces;
using Soteo.Gameplay.Nodes.Entities;
using Soteo.Shared.Packets;

namespace Soteo.Gameplay.Abilities;

public sealed class RecallAbility : Ability<RecallAbility>
{
    public override Scalable<float> StaticUseTime => 10;
    
    public override void TakeEffect(AbilityContext context)
    {
        base.TakeEffect(context);
        context.User.Remove();
        context.GetRequiredService<IPacketSender>()
            .SendReliable(new CharacterRecalledPacket { CharacterId = context.User.Id }, MasterServerId);
    }
}