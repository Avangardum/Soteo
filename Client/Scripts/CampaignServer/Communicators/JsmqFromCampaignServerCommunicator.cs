using Soteo.CampaignServer.GameState.DataObjects;
using Soteo.CampaignServer.Interfaces;
using Soteo.Shared;
using Soteo.Shared.Interfaces;
using Soteo.Shared.Packets;

namespace Soteo.CampaignServer.Communicators;

/// <summary>
/// Communicator using the JavaScript message queue instead of WebSockets. Used for singleplayer in browser.
/// </summary>
public sealed class JsmqFromCampaignServerCommunicator
(
    IPacketSerializer packetSerializer,
    IPacketHandler packetHandler,
    IUserRepository userRepo
) : Object, ICommunicator
{
    public void Poll()
    {
        while (true)
        {
            string? base64 = (string?)JavaScript.Eval($"""jsmq.receive("{CampaignServerId}")""");
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
            }
            else
            {
                User sender = userRepo[senderId];
                packetHandler.HandleAsync(packet, sender).CollectException();
            }
        }
    }
    
    public void SendTo(Packet packet, Guid receiverId)
    {
        byte[] bytes = [..CampaignServerId.ToByteArray(), ..packetSerializer.Serialize(packet)];
        string base64 = Convert.ToBase64String(bytes);
        JavaScript.Eval($"""jsmq.send("{base64}", "{receiverId}");""");
    }

    public void RelayFrom(RelayedPacket packet, Guid senderId)
    {
        SendTo(packet with { PeerId = senderId }, packet.PeerId);
    }
}