using Fika.Core.Networking.LiteNetLib.Utils;
using UnityEngine;

namespace LeaveItThere.FikaModule.Packets;

public struct PlacedItemStateChangedPacket : INetSerializable
{
    public string ItemId;
    public Vector3 Position;
    public Quaternion Rotation;
    public bool IsPlaced;
    public bool PhysicsEnableRequested;

    public void Deserialize(NetDataReader reader)
    {
        ItemId = reader.GetString();
        Position = reader.GetUnmanaged<Vector3>();
        Rotation = reader.GetUnmanaged<Quaternion>();
        IsPlaced = reader.GetBool();
        PhysicsEnableRequested = reader.GetBool();
    }

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(ItemId);
        writer.PutUnmanaged(Position);
        writer.PutUnmanaged(Rotation);
        writer.Put(IsPlaced);
        writer.Put(PhysicsEnableRequested);
    }
}
