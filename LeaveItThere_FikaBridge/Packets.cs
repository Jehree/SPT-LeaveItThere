using EFT;
using EFT.InventoryLogic;
using Fika.Core.Networking;
using LiteNetLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LeaveItThere_FikaBridge
{
    public struct SpawnPlacedItemPacket : INetSerializable
    {
        public Item Item;

        public void Deserialize(NetDataReader reader)
        {
            Item = reader.GetItem();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.PutItem(Item);
        }
    }

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
