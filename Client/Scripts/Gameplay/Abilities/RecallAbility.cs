using Soteo.Gameplay.Interfaces;
using Soteo.Gameplay.Nodes.Entities;
using Soteo.Shared.Packets;

namespace Soteo.Gameplay.Abilities;

public sealed class RecallAbility(Unit owner, IPacketSender packetSender) : UntargetedAbility(owner)
{
    public override float CastTimeSeconds => 10;
    
    public override void Cast()
    {
        owner.QueueFree();
        packetSender.SendReliable(new CharacterRecalledPacket { CharacterId = owner.Id }, MasterServerId);
    }
}