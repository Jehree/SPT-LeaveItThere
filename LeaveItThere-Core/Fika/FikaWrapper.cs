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
using SPT.Reflection.Utils;

namespace LeaveItThere.Fika
{
    internal class FikaWrapper
    {
        public static bool IAmHost()
        {
            return Singleton<FikaServer>.Instantiated;
        }

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
                Singleton<FikaServer>.Instance.SendDataToAll<PlacedItemStateChangedPacket>(ref packet, DeliveryMethod.ReliableOrdered);
            }
            if (Singleton<FikaClient>.Instantiated)
            {
                Singleton<FikaClient>.Instance.SendData<PlacedItemStateChangedPacket>(ref packet, DeliveryMethod.ReliableOrdered);
            }
        }

        public static string GetRaidId()
        {
            Plugin.LogSource.LogError($"GroupId: {FikaBackendUtils.GroupId}");
            Plugin.LogSource.LogError($"Main Player Id: {LITSession.Instance.Player.ProfileId}");
            Plugin.LogSource.LogError($"Session Id: {ClientAppUtils.GetMainApp().GetClientBackEndSession().Profile.ProfileId}");
            return FikaBackendUtils.GroupId;
        }

        public static void OnPlacedItemStateChangedPacketReceived(PlacedItemStateChangedPacket packet, NetPeer peer)
        {
            if (packet.IsPlaced)
            {
                PlaceItemPacketReceived(packet);
            }
            else
            {
                ReclaimItemPackedReceived(packet);
            }

            if (IAmHost())
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
                fakeItem.gameObject.GetComponent<MoveableObject>().EnablePhysics();
            }

            ObjectMover mover = ObjectMover.Instance;
            if (mover.Enabled && mover.Target.gameObject == fakeItem.gameObject)
            {
                mover.Disable(false);
            }
        }

        private static void ReclaimItemPackedReceived(PlacedItemStateChangedPacket packet)
        {
            FakeItem fakeItem = LITSession.Instance.GetFakeItemOrNull(packet.ItemId);
            if (fakeItem == null) return;

            ObjectMover mover = ObjectMover.Instance;
            if (mover.Enabled && mover.Target.gameObject == fakeItem.gameObject)
            {
                mover.Disable(false);
            }

            fakeItem.Reclaim();
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
