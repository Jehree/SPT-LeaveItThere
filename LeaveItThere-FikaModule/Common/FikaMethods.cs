﻿using Comfort.Common;
using EFT.Interactive;
using EFT.InventoryLogic;
using Fika.Core.Coop.Components;
using Fika.Core.Modding;
using Fika.Core.Modding.Events;
using Fika.Core.Networking;
using LeaveItThere.Components;
using LeaveItThere.FikaModule.Packets;
using LeaveItThere.Helpers;
using LiteNetLib;
using System;
using UnityEngine;

namespace LeaveItThere.FikaModule.Common
{
    internal class FikaMethods
    {
        public static void SendPlacedStateChangedPacket(FakeItem fakeItem, bool isPlaced, bool physicsEnableRequested = false)
        {
            PlacedItemStateChangedPacket packet = new PlacedItemStateChangedPacket
            {
                ItemId = fakeItem.ItemId,
                Position = fakeItem.gameObject.transform.position,
                Rotation = fakeItem.gameObject.transform.rotation,
                IsPlaced = isPlaced,
                PhysicsEnableRequested = physicsEnableRequested
            };
            if (Singleton<FikaServer>.Instantiated)
            {
                Singleton<FikaServer>.Instance.SendDataToAll(ref packet, DeliveryMethod.ReliableOrdered);
            }
            if (Singleton<FikaClient>.Instantiated)
            {
                Singleton<FikaClient>.Instance.SendData(ref packet, DeliveryMethod.ReliableOrdered);
            }
        }

        private static void OnPlacedItemStateChangedPacketReceived(PlacedItemStateChangedPacket packet)
        {
            if (packet.IsPlaced)
            {
                PlaceItemPacketReceived(packet);
            }
            else
            {
                ReclaimItemPackedReceived(packet);
            }

            if (Main.IAmHost())
            {
                Singleton<FikaServer>.Instance.SendDataToAll(ref packet, LiteNetLib.DeliveryMethod.ReliableOrdered);
            }
        }

        private static void PlaceItemPacketReceived(PlacedItemStateChangedPacket packet)
        {
            ObservedLootItem lootItem = ItemHelper.GetLootItem(packet.ItemId) as ObservedLootItem;

            FakeItem fakeItem;
            if (LITSession.Instance.TryGetFakeItem(packet.ItemId, out FakeItem fakeItemFetch))
            {
                fakeItem = fakeItemFetch;
            }
            else
            {
                fakeItem = FakeItem.CreateNewFakeItem(lootItem);
            }

            fakeItem.PlaceAtPosition(packet.Position, packet.Rotation);

            if (packet.PhysicsEnableRequested)
            {
                fakeItem.gameObject.GetComponent<Moveable>().EnablePhysics(true);
            }

            ItemMover mover = ItemMover.Instance;
            if (mover.enabled && mover.Target.gameObject == fakeItem.gameObject)
            {
                mover.Disable(false);
            }
        }

        private static void ReclaimItemPackedReceived(PlacedItemStateChangedPacket packet)
        {
            FakeItem fakeItem = LITSession.Instance.GetFakeItemOrNull(packet.ItemId);
            if (fakeItem == null) return;

            ItemMover mover = ItemMover.Instance;
            if (mover.enabled && mover.Target.gameObject == fakeItem.gameObject)
            {
                mover.Disable(false);
            }

            fakeItem.Reclaim();
        }

        public static void OnFikaNetManagerCreated(FikaNetworkManagerCreatedEvent managerCreatedEvent)
        {
            managerCreatedEvent.Manager.RegisterPacket<PlacedItemStateChangedPacket>(OnPlacedItemStateChangedPacketReceived);
            managerCreatedEvent.Manager.RegisterPacket<LITSpawnItemPacket, NetPeer>(SpawnItemPacketReceived);
            managerCreatedEvent.Manager.RegisterPacket<LITGenericPacket, NetPeer>(GenericPacketTools.OnGenericPacketReceived);
        }

        public static void InitOnPluginEnabled()
        {
            FikaEventDispatcher.SubscribeEvent<FikaNetworkManagerCreatedEvent>(OnFikaNetManagerCreated);
        }

        public static void SendSpawnItemPacket(Item item, Vector3 position, Quaternion rotation, Action<LootItem> senderCallback)
        {
            // sender always spawns the item themselves, so that they can pass the callback into the helper
            ItemHelper.SpawnItem(item, position, rotation, senderCallback);

            LITSpawnItemPacket packet = new()
            {
                Item = item,
                Position = position,
                Rotation = rotation
            };

            if (Main.IAmHost())
            {
                // if the host is spawning the item, all peers need to told to do the same
                Singleton<FikaServer>.Instance.SendDataToAll(ref packet, DeliveryMethod.ReliableOrdered);
            }
            else
            {
                // if a client is spawning the item, the host needs to be told about it
                // the host will not return a packet to the sender
                Singleton<FikaClient>.Instance.SendData(ref packet, DeliveryMethod.ReliableOrdered);
            }
        }

        private static void SpawnItemPacketReceived(LITSpawnItemPacket packet, NetPeer peer)
        {
            ItemHelper.SpawnItem(packet.Item, packet.Position, packet.Rotation);

            if (!Main.IAmHost()) return;

            FikaServer fikaServer = Singleton<FikaServer>.Instance;
            NetManager netServer = fikaServer.NetServer;

            foreach (NetPeer p in netServer.ConnectedPeerList)
            {
                // do not return a packet to the sender, they already spawned the item locally
                if (p == peer) continue;

                fikaServer.SendDataToPeer(p, ref packet, DeliveryMethod.ReliableOrdered);
            }
        }
    }
}
