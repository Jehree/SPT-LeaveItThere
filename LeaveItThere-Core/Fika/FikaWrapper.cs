using Comfort.Common;
using EFT.Interactive;
using Fika.Core.Coop.Utils;
using Fika.Core.Modding;
using Fika.Core.Modding.Events;
using Fika.Core.Networking;
using LeaveItThere.Components;
using LeaveItThere.Helpers;
using LeaveItThere.Packets;
using LiteNetLib;

namespace LeaveItThere.Fika
{
    internal class FikaWrapper
    {
        public static bool IAmHost()
        {
            return Singleton<FikaServer>.Instantiated;
        }

        public static void SendPlacedStateChangedPacket(FakeItem fakeItem, bool physicsEnableRequested = false)
        {
            PlacedItemStateChangedPacket packet = new PlacedItemStateChangedPacket
            {
                ItemId = fakeItem.ItemId,
                Position = fakeItem.WorldPosition,
                Rotation = fakeItem.WorldRotation,
                IsPlaced = fakeItem.Placed,
                PhysicsEnableRequested = physicsEnableRequested
            };
            if (Singleton<FikaServer>.Instantiated)
            {
                Singleton<FikaServer>.Instance.SendDataToAll<PlacedItemStateChangedPacket>(ref packet, DeliveryMethod.ReliableOrdered);
            }
            if (Singleton<FikaClient>.Instantiated)
            {
                Singleton<FikaClient>.Instance.SendData<PlacedItemStateChangedPacket>(ref packet, DeliveryMethod.ReliableOrdered);
            }
        }

        public static string GetRaidId()
        {
            return FikaBackendUtils.GroupId;
        }

        public static void OnPlacedItemStateChangedPacketReceived(PlacedItemStateChangedPacket packet, NetPeer peer)
        {
            ObservedLootItem lootItem = ItemHelper.GetLootItem(packet.ItemId) as ObservedLootItem;

            FakeItem fakeItem;
            if (ModSession.Instance.TryGetFakeItem(packet.ItemId, out FakeItem fakeItemFetch))
            {
                fakeItem = fakeItemFetch;
            }
            else
            {
                fakeItem = FakeItem.CreateNewRemoteInteractable(lootItem);
            }

            // always place to ensure that location is synced, THEN reclaim if needed
            fakeItem.PlaceAtLocation(packet.Position, packet.Rotation);
            if (!packet.IsPlaced)
            {
                fakeItem.Reclaim();
            }

            if (packet.PhysicsEnableRequested)
            {
                fakeItem.gameObject.GetComponent<MoveableObject>().EnablePhysics();
            }

            var mover = ObjectMover.GetMover();
            if (mover.Enabled && mover.Target.gameObject == fakeItem.gameObject)
            {
                mover.Disable(false);
            }

            if (IAmHost())
            {
                Singleton<FikaServer>.Instance.SendDataToAll(ref packet, LiteNetLib.DeliveryMethod.ReliableOrdered);
            }
        }

        public static void OnFikaNetManagerCreated(FikaNetworkManagerCreatedEvent managerCreatedEvent)
        {
            managerCreatedEvent.Manager.RegisterPacket<PlacedItemStateChangedPacket, NetPeer>(OnPlacedItemStateChangedPacketReceived);
        }

        public static void InitOnPluginEnabled()
        {
            FikaEventDispatcher.SubscribeEvent<FikaNetworkManagerCreatedEvent>(OnFikaNetManagerCreated);
        }
    }
}
