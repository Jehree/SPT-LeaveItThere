using EFT.InventoryLogic;
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
        public bool PhysicsEnableRequested;

        public void Deserialize(NetDataReader reader)
        {
            ItemId = reader.GetString();
            Position = reader.GetVector3();
            Rotation = reader.GetQuaternion();
            IsPlaced = reader.GetBool();
            PhysicsEnableRequested = reader.GetBool();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(ItemId);
            //writer.Put(Position);
            //writer.Put(Rotation);
            writer.Put(IsPlaced);
            writer.Put(PhysicsEnableRequested);
        }
    }

    public struct LITSpawnItemPacket : INetSerializable
    {
        public Item Item;
        public Vector3 Position;
        public Quaternion Rotation;

        public void Deserialize(NetDataReader reader)
        {
            Item = reader.GetItem();
            Position = reader.GetVector3();
            Rotation = reader.GetQuaternion();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutItem(Item);
            //writer.Put(Position);
            //writer.Put(Rotation);
        }
    }

    public struct LITRemoveItemFromContainerPacket : INetSerializable
    {
        public Item Container;
        public Item ItemToRemove;
        public void Deserialize(NetDataReader reader)
        {
            Container = reader.GetItem();
            ItemToRemove = reader.GetItem();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutItem(Container);
            writer.PutItem(ItemToRemove);
        }
    }

    public struct LITSpawnItemInContainerPacket : INetSerializable
    {
        public Item Container;
        public Item Item;
        public void Deserialize(NetDataReader reader)
        {
            Container = reader.GetItem();
            Item = reader.GetItem();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutItem(Container);
            writer.PutItem(Item);
        }
    }
}
