using Fika.Core.Networking;
using LiteNetLib.Utils;

namespace LeaveItThere_Packets
{
    public struct LITGenericPacket : INetSerializable
    {
        public string PacketGUID;
        public int Destination;
        public string SenderProfileId;
        public bool BoolData;

        public bool StringDataIncluded;
        public string StringData;

        public bool ByteArrayDataIncluded;
        public byte[] ByteArrayData;

        public void Deserialize(NetDataReader reader)
        {
            PacketGUID = reader.GetString();
            SenderProfileId = reader.GetString();
            Destination = reader.GetInt();
            BoolData = reader.GetBool();

            StringDataIncluded = reader.GetBool();
            if (StringDataIncluded)
            {
                StringData = reader.GetString();
            }

            ByteArrayDataIncluded = reader.GetBool();
            if (ByteArrayDataIncluded)
            {
                ByteArrayData = reader.GetByteArray();
            }
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(PacketGUID);
            writer.Put(SenderProfileId);
            writer.Put(Destination);
            writer.Put(BoolData);

            writer.Put(StringDataIncluded);
            if (StringDataIncluded)
            {
                writer.Put(StringData);
            }
            
            writer.Put(ByteArrayDataIncluded);
            if (ByteArrayDataIncluded)
            {
                writer.Put(ByteArrayData);
            }
        }
    }
}
