#nullable enable

using Fika.Core.Networking;
using LiteNetLib.Utils;

namespace LeaveItThere_Packets
{
    public struct LITGenericPacket : INetSerializable
    {
        public string PacketGUID;
        public int Destination;
        public string SenderProfileId;
        public string? StringData;
        public bool? BoolData;
        public byte[]? ByteArrayData;

        public void Deserialize(NetDataReader reader)
        {
            PacketGUID = reader.GetString();
            SenderProfileId = reader.GetString();
            Destination = reader.GetInt();
            StringData = reader.GetString();
            BoolData = reader.GetBool();
            ByteArrayData = reader.GetByteArray();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(PacketGUID);
            writer.Put(SenderProfileId);
            writer.Put(Destination);
            writer.Put(StringData ?? "");
            writer.Put(BoolData ?? false);
            writer.Put(ByteArrayData ?? [0]);
        }
    }
}
