using Soteo.Core.CampaignServer.Interfaces;
using Soteo.Core.Shared;
using Soteo.Core.Shared.Interfaces;
using Soteo.Core.Shared.Packets;
using Soteo.Gameplay.Interfaces;
using Soteo.Shared;
using Soteo.Util;

namespace Soteo.CampaignServer.Communicators;

/// <summary>
/// Communicator using the JavaScript message queue instead of WebSockets. Used for singleplayer in browser.
/// </summary>
public sealed class JsmqFromCampaignServerCommunicator
(
    IPacketSerializer packetSerializer,
    IPacketHandler packetHandler,
    IUserRepository userRepo
) : GdObject, ICommunicator
{
    private readonly HashSet<Guid> _peerIds = [];
    
    public void Poll()
    {
        while (true)
        {
            var base64 = (string?)JavaScript.Eval($"""jsmq.receive("{Const.CampaignServerId}")""");
            if (base64 == null) return;
            byte[] bytes = Convert.FromBase64String(base64);
            var senderId = new Guid(bytes.AsSpan()[..Const.BytesInGuid].ToArray());
            Packet packet = packetSerializer.Deserialize(bytes.AsSpan()[Const.BytesInGuid..]);
            if (packet is CampaignServerHandshakePacket handshake)
            {
                var claims = new Dictionary<string, object>
                {
                    ["sub"] = senderId.ToString(),
                    // When using JSMQ, role is sent instead of token
                    [handshake.Token] = true,
                };
                userRepo.OnConnected(claims);
                _peerIds.Add(senderId);
            }
            else
            {
                packetHandler.HandleAsync(packet, senderId).CollectException();
            }
        }
    }
    
    public void SendTo(Packet packet, Guid receiverId)
    {
        byte[] bytes = [..Const.CampaignServerId.ToByteArray(), ..packetSerializer.Serialize(packet)];
        string base64 = Convert.ToBase64String(bytes);
        JavaScript.Eval($"""jsmq.send("{base64}", "{receiverId}");""");
    }

    public void Broadcast(Packet packet)
    {
        byte[] bytes = [..Const.CampaignServerId.ToByteArray(), ..packetSerializer.Serialize(packet)];
        string base64 = Convert.ToBase64String(bytes);
        foreach (Guid id in _peerIds)
            JavaScript.Eval($"""jsmq.send("{base64}", "{id}");""");
    }

    public void RelayFrom(RelayedPacket packet, Guid senderId)
    {
        SendTo(packet with { PeerId = senderId }, packet.PeerId);
    }
}
