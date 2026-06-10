using Soteo.Core.Packets;
using Soteo.Main.Gameplay.Interfaces;

namespace Soteo.Main.Gameplay.Services.Communicators;

public sealed class ChunkCollector : IChunkCollector
{
    private readonly Dictionary<Guid, Group> _groups = [];
    private DateTime _nextCleanupTime = DateTime.UtcNow;

    /// <inheritdoc />
    public byte[]? AddChunk(ChunkPacket chunk, Guid senderId)
    {
        if (DateTime.UtcNow >= _nextCleanupTime)
            CleanupTimedOutGroups();
        
        if (_groups.TryGetValue(chunk.GroupId, out Group? group))
        {
            if (senderId != group.SenderId) return null;
            if (group.IsTimedOut) return null;
            group.Chunks.Add(chunk);
        }
        else
        {
            DateTime timeOutAfter = DateTime.UtcNow.AddSeconds(10);
            _groups[chunk.GroupId] = group = new Group(senderId, timeOutAfter, [chunk]);
        }
        
        if (chunk.IsLast)
            group.TargetSize = chunk.Index + 1;
        
        if (group.Chunks.Count == group.TargetSize)
        {
            byte[] restoredPacketBytes = group.Chunks
                .OrderBy(it => it.Index)
                .SelectMany(it => it.Bytes)
                .ToArray();
            _groups.Remove(chunk.GroupId);
            return restoredPacketBytes;
        }
        
        return null;
    }
    
    private void CleanupTimedOutGroups()
    {
        List<Guid> timedOutGroupIds = _groups
            .Where(it => it.Value.IsTimedOut)
            .Select(it => it.Key)
            .ToList();
        foreach (Guid id in timedOutGroupIds)
            _groups.Remove(id);
        _nextCleanupTime = DateTime.UtcNow.AddMinutes(1);
    }
    
    private record Group(Guid SenderId, DateTime TimeOutAfter, List<ChunkPacket> Chunks)
    {
        public int? TargetSize { get; set; }
        public bool IsTimedOut => DateTime.UtcNow > TimeOutAfter;
    }
}
