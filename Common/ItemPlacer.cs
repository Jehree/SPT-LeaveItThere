using EFT.Interactive;
using InteractableInteractionsAPI.Common;
using LeaveItThere.Components;
using UnityEngine;
using Comfort.Common;
using EFT;
using LeaveItThere.Helpers;
using Diz.LanguageExtensions;
using EFT.InventoryLogic;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using EFT.UI;
using System.Reflection;

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
            var dataPack = new PlacedItemDataPack(mapId, dataList);
            SPTServerHelper.ServerRoute(DataToServerURL, dataPack);
        }

        public static CustomInteraction GetPlaceItemAction(LootItem lootItem)
        {
            var session = ModSession.GetSession();
            bool allowed = session.PlacementIsAllowed(lootItem.Item);
            return new CustomInteraction(
            "Place Item",
            !allowed,
            () =>
            {
                if (allowed)
                {
                    PlaceItem(lootItem as ObservedLootItem, lootItem.gameObject.transform.position);
                    session.PointsSpent += ItemHelper.GetItemCost(lootItem.Item);
                    PlacedPlayerFeedback(lootItem.Item);
                }
                else
                {
                    NotificationManagerClass.DisplayWarningNotification("Maximum item placement cost for this map already met!");
                    InteractionHelper.RefreshPrompt();
                }
            });
        }

        public static void PlaceItem(ObservedLootItem lootItem, Vector3 location)
        {
            var session = ModSession.GetSession();
            lootItem.gameObject.transform.position = new Vector3(0, -99999, 0);
            RemoteInteractable remoteInteractable = RemoteInteractable.GetOrCreateRemoteInteractable(location, lootItem, session.GetPairOrNull(lootItem));
            session.AddOrUpdatePair(lootItem, remoteInteractable, location, true);
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
