using Microsoft.Extensions.DependencyInjection;
using Soteo.Core.Gameplay.Dto;
using Soteo.Core.Gameplay.Enums;
using Soteo.Core.Gameplay.Interfaces;
using Soteo.Core.Shared;
using Soteo.Core.Shared.Packets;
using Soteo.Util;

namespace Soteo.Core.Gameplay.Abilities;

public sealed class RecallAbility : Ability
{
    public override CanTarget Targeting => CanTarget.Nothing;
    public override bool LoopAnimation => true;
    public override Scalable<double> StaticUseTime => 10;
    
    public override void TakeEffect(AbilityContext context)
    {
        base.TakeEffect(context);
        context.User.Remove();
        context.GetRequiredService<IPacketSender>()
            .SendReliable(new CharacterRecalledPacket { CharacterId = context.User.Id }, Const.CampaignServerId);
    }
}
