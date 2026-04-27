using Microsoft.Extensions.DependencyInjection;
using Soteo.Gameplay.Dto;
using Soteo.Gameplay.Interfaces;
using Soteo.Gameplay.Util;
using Soteo.Shared.Packets;

namespace Soteo.Gameplay.Abilities;

public sealed class RecallAbility : Ability
{
    public override bool LoopAnimation => true;
    public override Scalable<float> StaticUseTime => 10;
    
    public override void TakeEffect(AbilityContext context)
    {
        base.TakeEffect(context);
        context.User.Remove();
        context.GetRequiredService<IPacketSender>()
            .SendReliable(new CharacterRecalledPacket { CharacterId = context.User.Id }, MasterServerId);
    }
}