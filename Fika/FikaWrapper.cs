using Comfort.Common;
using Fika.Core.Coop.Utils;
using Fika.Core.Modding;
using Fika.Core.Modding.Events;
using Fika.Core.Networking;
using LeaveItThere.Common;
using LeaveItThere.Components;
using LeaveItThere_FikaBridge;
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

        //public static void OnSpawnItemPacketReceived(SpawnPlacedItemPacket packet, NetPeer peer){}

        public static void OnPlacedItemStateChangedPacketReceived(PlacedItemStateChangedPacket packet, NetPeer peer)
        {
            var session = ModSession.GetSession();
            var pair = session.GetPairOrNull(packet.ItemId);

            if (pair == null)
            {
                throw new System.Exception($"Received a PlacedItemStateChangedPacket with an Item Id ({packet.ItemId}) that peer: {peer.ToString()} did not have a pair for!");
            }

            if (packet.IsPlaced)
            {
                // re-placing is necessary because if an item is placed, placing it again updates it's position
                ItemPlacer.PlaceItem(pair.LootItem, pair.PlacementPosition, pair.PlacementRotation);
            }
            else
            {
                // not in this case, but DemolishItem() is safe and won't do anything if the item is not placed
                pair.RemoteInteractable.DemolishItem();
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
