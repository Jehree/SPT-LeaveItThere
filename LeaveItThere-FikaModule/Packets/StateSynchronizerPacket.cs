using Fika.Core.Networking.LiteNetLib.Utils;

namespace LeaveItThere.FikaModule.Packets;

internal struct StateSynchronizerPacket : INetSerializable
{
    public bool IsFakeItem;
    public string ItemId;
    public string DatabaseKey;
    public string Data;

    public void Deserialize(NetDataReader reader)
    {
        IsFakeItem = reader.GetBool();
        ItemId = reader.GetString();
        DatabaseKey = reader.GetString();
        Data = reader.GetString();
    }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(IsFakeItem);
        writer.Put(ItemId);
        writer.Put(DatabaseKey);
        writer.Put(Data);
    }
}
