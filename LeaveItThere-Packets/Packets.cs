using Fika.Core.Networking;
using LiteNetLib.Utils;
using UnityEngine;

namespace LeaveItThere.Packets
{
    public struct PlacedItemStateChangedPacket : INetSerializable
    {
        public string ItemId;
        public Vector3 Position;
        public Quaternion Rotation;
        public bool IsPlaced;

        public void Deserialize(NetDataReader reader)
        {
            ItemId = reader.GetString();
            Position = reader.GetVector3();
            Rotation = reader.GetQuaternion();
            IsPlaced = reader.GetBool();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(ItemId);
            writer.Put(Position);
            writer.Put(Rotation);
            writer.Put(IsPlaced);
        }
    }
}
