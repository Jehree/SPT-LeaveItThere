using EFT.Interactive;
using InteractableInteractionsAPI.Common;
using LeaveItThere.Components;
using UnityEngine;
using Comfort.Common;
using EFT;
using LeaveItThere.Helpers;
using EFT.InventoryLogic;
using System.Collections.Generic;
using EFT.UI;
using LeaveItThere.Fika;

namespace LeaveItThere.Common
{
    internal class ItemPlacer
    {
        public static string DataToServerURL = "/jehree/pip/data_to_server";
        public static string DataToClientURL = "/jehree/pip/data_to_client";

        public static void SendPlacedItemDataToServer()
        {
            var dataList = new List<PlacedItemData>();
            string mapId = Singleton<GameWorld>.Instance.LocationId;
            List<string> placedItemInstanceIds = new();
            foreach (var pair in ModSession.GetSession().ItemRemotePairs)
            {
                if (pair.Placed == false) continue;
                dataList.Add(new PlacedItemData(pair.LootItem.Item, pair.PlacementPosition, pair.PlacementRotation));
                placedItemInstanceIds.Add(pair.LootItem.ItemId);
            }
            var dataPack = new PlacedItemDataPack(FikaInterface.GetRaidId(), mapId, dataList);
            SPTServerHelper.ServerRoute(DataToServerURL, dataPack);
        }

        public static CustomInteraction GetPlaceItemAction(LootItem lootItem)
        {
            string itemId = lootItem.ItemId;

            return new CustomInteraction(
            "Place Item",
            !ModSession.GetSession().PlacementIsAllowed(lootItem.Item),
            () =>
            {
                var session = ModSession.GetSession();
                var observedLootItem = ItemHelper.GetLootItem(itemId) as ObservedLootItem;
                bool allowed = session.PlacementIsAllowed(observedLootItem.Item);

                if (allowed)
                {
                    PlaceItem(observedLootItem, observedLootItem.gameObject.transform.position, observedLootItem.gameObject.transform.rotation);
                    PlacedPlayerFeedback(observedLootItem.Item);
                    FikaInterface.SendPlacedStateChangedPacket(session.GetPairOrNull(observedLootItem));
                }
                else
                {
                    NotificationManagerClass.DisplayWarningNotification("Maximum item placement cost for this map already met!");
                    InteractionHelper.RefreshPrompt();
                }
            });
        }

        public static void PlaceItem(ObservedLootItem lootItem, Vector3 position, Quaternion rotation)
        {
            var session = ModSession.GetSession();
            var pair = session.GetPairOrNull(lootItem.ItemId);

            bool newPairNeeded = pair == null; 
            bool pairUpdateNeeded = !newPairNeeded;
            bool pointUpdateNeeded = newPairNeeded || !pair.Placed; 

            if (newPairNeeded)
            {
                RemoteInteractable interactable = RemoteInteractable.CreateNewRemoteInteractable(position, rotation, lootItem);
                pair = session.AddPair(lootItem, interactable, position, rotation, true);
            }

            if (pairUpdateNeeded)
            {
                pair = session.UpdatePair(pair, position, rotation, true);
            }

            if (pointUpdateNeeded)
            {
                session.PointsSpent += ItemHelper.GetItemCost(lootItem.Item);
            }

            lootItem.gameObject.transform.position = new Vector3(0, -99999, 0);
            lootItem.gameObject.transform.rotation = rotation;
            pair.RemoteInteractable.gameObject.transform.position = position;
            pair.RemoteInteractable.gameObject.transform.rotation = rotation;

            lootItem.gameObject.GetOrAddComponent<Rigidbody>().isKinematic = true;
            pair.RemoteInteractable.gameObject.GetOrAddComponent<Rigidbody>().isKinematic = true;
        }

        public static void PlacedPlayerFeedback(Item item)
        {
            var session = ModSession.GetSession();
            if (Settings.CostSystemEnabled.Value)
            {
                InteractionHelper.NotificationLong($"Placement cost: {ItemHelper.GetItemCost(item)}, {Settings.GetAllottedPoints() - session.PointsSpent} out of {Settings.GetAllottedPoints()} points remaining");
            }
            Singleton<GUISounds>.Instance.PlayUISound(EUISoundType.MenuWeaponAssemble);
        }
    }
}
