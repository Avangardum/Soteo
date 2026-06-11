using Soteo.Core;
using Soteo.Core.Interfaces;
using Soteo.Core.Packets;

namespace Soteo.Main.CampaignServer.Communicators;

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
        string base64 = ToBase64(packet);
        JavaScript.Eval($"""jsmq.send("{base64}", "{receiverId}");""");
    }

    public void BroadcastToShardServersAndClients(Packet packet)
    {
        string base64 = ToBase64(packet);
        foreach (Guid id in _peerIds)
            JavaScript.Eval($"""jsmq.send("{base64}", "{id}");""");
    }
    
    public void BroadcastToShardServers(Packet packet)
    {
        string base64 = ToBase64(packet);
        foreach (Guid id in userRepo.Values.Where(it => it.IsShard).Select(it => it.Id))
            JavaScript.Eval($"""jsmq.send("{base64}", "{id}");""");
    }
    
    public void BroadcastToClients(Packet packet)
    {
        string base64 = ToBase64(packet);
        foreach (Guid id in userRepo.Values.Where(it => it.IsPlayer).Select(it => it.Id))
            JavaScript.Eval($"""jsmq.send("{base64}", "{id}");""");
    }

    private string ToBase64(Packet packet)
    {
        byte[] bytes = [..Const.CampaignServerId.ToByteArray(), ..packetSerializer.Serialize(packet)];
        return Convert.ToBase64String(bytes);
    }
    
    public void RelayFrom(RelayedPacket packet, Guid senderId)
    {
        SendTo(packet with { PeerId = senderId }, packet.PeerId);
    }
}
