using System.Threading.Tasks;
using Soteo.Shared.Packets.Shared;

namespace Soteo.Client;

public interface IPacketHandler
{
    Task HandleAsync(Packet packet, Guid senderId);
}