using Comfort.Common;
using EFT.Interactive;
using EFT.UI;
using Fika.Core.Coop.Utils;
using Fika.Core.Modding;
using Fika.Core.Modding.Events;
using Fika.Core.Networking;
using LeaveItThere.Common;
using LeaveItThere.Components;
using LeaveItThere.Packets;
using LeaveItThere.Helpers;
using LiteNetLib;

namespace LeaveItThere.Fika
{
    internal class FikaWrapper
    {
        public static bool IAmHost()
        {
            return Singleton<FikaServer>.Instantiated;
        }

        public static void SendPlacedStateChangedPacket(ItemRemotePair pair)
        {
            PlacedItemStateChangedPacket packet = new PlacedItemStateChangedPacket
            {
                ItemId = pair.LootItem.ItemId,
                Position = pair.PlacementPosition,
                Rotation = pair.PlacementRotation,
                IsPlaced = pair.Placed
            };
            if (Singleton<FikaServer>.Instantiated)
            {
                Singleton<FikaServer>.Instance.SendDataToAll<PlacedItemStateChangedPacket>(ref packet, LiteNetLib.DeliveryMethod.ReliableOrdered);
            }
            if (Singleton<FikaClient>.Instantiated)
            {
                Singleton<FikaClient>.Instance.SendData<PlacedItemStateChangedPacket>(ref packet, LiteNetLib.DeliveryMethod.ReliableOrdered);
            }
        }

        public static string GetRaidId()
        {
            return FikaBackendUtils.GroupId;
        }

        public static void OnPlacedItemStateChangedPacketReceived(PlacedItemStateChangedPacket packet, NetPeer peer)
        {
            ObservedLootItem lootItem = ItemHelper.GetLootItem(packet.ItemId) as ObservedLootItem;
            var pair = ModSession.GetSession().GetPairOrNull(packet.ItemId);

            if (packet.IsPlaced)
            {
                // re-placing is necessary because if an item is placed, placing it again updates its position
                ItemPlacer.PlaceItem(lootItem, packet.Position, packet.Rotation);
            }
            else
            {
                if (pair == null)
                {
                    string err = "Something is wrong! packet.IsPlaced was false, but the pair couldn't be found. This shouldn't happen!";
                    ConsoleScreen.LogError(err);
                    throw new System.Exception(err);
                }

                pair.RemoteInteractable.DemolishItem();
            }

            var mover = ObjectMover.GetMover();
            if (mover.Enabled && mover.Target == pair.RemoteInteractable.gameObject)
            {
                mover.Disable(false);
            }

            if (Singleton<FikaServer>.Instantiated)
            {
                Singleton<FikaServer>.Instance.SendDataToAll<PlacedItemStateChangedPacket>(ref packet, LiteNetLib.DeliveryMethod.ReliableOrdered);
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
