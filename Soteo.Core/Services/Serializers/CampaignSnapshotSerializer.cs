using Soteo.Core.Dto.Snapshots;
using Soteo.Core.Interfaces;

namespace Soteo.Core.Services.Serializers;

public sealed class CampaignSnapshotSerializer(ISerializationHelper s) : ICampaignSnapshotSerializer
{
    public byte[] Serialize(CampaignSnapshot snapshot)
    {
        var stream = new MemoryStream();
        SerializeCampaignServerSnapshot(snapshot.CampaignServer, stream);
        s.SerializeDictionary(snapshot.Shards, s.SerializeGuid, s.SerializeShardSnapshot, stream);
        return stream.ToArray();
    }
    
    public CampaignSnapshot Deserialize(byte[] bytes)
    {
        var stream = new MemoryStream(bytes);
        return new()
        {
            CampaignServer = DeserializeCampaignServerSnapshot(stream),
            Shards = s.DeserializeDictionary(s.DeserializeGuid, s.DeserializeShardSnapshot, stream),
        };
    }
    
    private void SerializeCampaignServerSnapshot(CampaignServerSnapshot value, Stream stream)
    {
        s.SerializeIndexedDictionary(value.Users, SerializeUserSnapshot, stream);
        s.SerializeIndexedDictionary(value.PlayerCharacterTrackers, SerializePlayerCharacterTrackerSnapshot, stream);
    }
    
    private CampaignServerSnapshot DeserializeCampaignServerSnapshot(Stream stream)
    {
        var snapshot = new CampaignServerSnapshot
        {
            Users = s.DeserializeIndexedDictionary(DeserializeUserSnapshot, it => it.Id, stream),
            PlayerCharacterTrackers =
                s.DeserializeIndexedDictionary(DeserializePlayerCharacterTrackerSnapshot, it => it.Id, stream),
        };
        return snapshot;
    }
    
    private void SerializeUserSnapshot(UserSnapshot value, Stream stream)
    {
        s.SerializeGuid(value.Id, stream);
        s.SerializeBool(value.IsConnected, stream);
        s.SerializeBool(value.IsPlayer, stream);
        s.SerializeBool(value.IsShard, stream);
    }
    
    private UserSnapshot DeserializeUserSnapshot(Stream stream)
    {
        return new()
        {
            Id = s.DeserializeGuid(stream),
            IsConnected = s.DeserializeBool(stream),
            IsPlayer = s.DeserializeBool(stream),
            IsShard = s.DeserializeBool(stream),
        };
    }
    
    private void SerializePlayerCharacterTrackerSnapshot(PlayerCharacterTrackerSnapshot value, Stream stream)
    {
        s.SerializeGuid(value.Id, stream);
        s.SerializeNullableStruct(value.PlayerId, s.SerializeGuid, stream);
        s.SerializeNullableStruct(value.ShardId, s.SerializeGuid, stream);
    }
    
    private PlayerCharacterTrackerSnapshot DeserializePlayerCharacterTrackerSnapshot(Stream stream)
    {
        return new()
        {
            Id = s.DeserializeGuid(stream),
            PlayerId = s.DeserializeNullableStruct(s.DeserializeGuid, stream),
            ShardId = s.DeserializeNullableStruct(s.DeserializeGuid, stream),
        };
    }
}
