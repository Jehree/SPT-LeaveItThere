using LiteNetLib.Utils;

namespace LeaveItThere.FikaModule.Packets
{
    public struct LITGenericPacket : INetSerializable
    {
        public string PacketGUID;
        public int Destination;
        public string SenderProfileId;
        public string Data;

        public void Deserialize(NetDataReader reader)
        {
            PacketGUID = reader.GetString();
            SenderProfileId = reader.GetString();
            Destination = reader.GetInt();
            Data = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(PacketGUID);
            writer.Put(SenderProfileId);
            writer.Put(Destination);
            writer.Put(Data);
        }
    }
}
