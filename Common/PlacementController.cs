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
    internal class PlacementController
    {
        private static string DataToServerURL = "/jehree/pip/data_to_server";
        private static string DataToClientURL = "/jehree/pip/data_to_client";

        public static void OnRaidStart()
        {
            string mapId = Singleton<GameWorld>.Instance.LocationId; 
            PlacedItemDataPack dataPack = SPTServerHelper.ServerRoute<PlacedItemDataPack>(DataToClientURL, new PlacedItemDataPack(mapId));
            foreach (var data in dataPack.ItemTemplates)
            {
                ItemHelper.SpawnItem(data.Item, new Vector3(0, -9999, 0), data.Rotation,
                (LootItem lootItem) =>
                {
                    PlaceItem(lootItem as ObservedLootItem, data.Location);
                    if (lootItem.Item is SearchableItemItemClass)
                    {
                        ItemHelper.MakeSearchableItemFullySearched(lootItem.Item as SearchableItemItemClass);
                    }
                    ModSession.GetSession().PointsSpent += GetItemCost(lootItem.Item);
                });
            }
        }

        public static void SendPlacedItemDataToServer()
        {
            /*
            if (_idFieldInfo == null && lostInsuredItems.Any())
            {
                // lazy cacheing should be fine here.. I don't think it'll take THAT long to find it anyway
                _idFieldInfo = lostInsuredItems[0].GetType().GetField("_id");
            }
            */

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

            /*
            object[]
            foreach (var lostItem in lostInsuredItems)
            {
                string _id = _idFieldInfo.GetValue(lostItem) as string;
                if (placedItemInstanceIds.Contains(_id))
                {
                    lostInsuredItems.RemoveFirst()
                }
            }

            for (int i = lostInsuredItems.Length - 1; i >= 0; i--)
            {
                object lostItem = lostInsuredItems[i];
                string _id = _idFieldInfo.GetValue(lostItem) as string;
                if (placedItemInstanceIds.Contains(_id))
                {
                    lostInsuredItems.Rem;
                }
            }
            */

        }

        public static CustomInteraction GetRemoteOpenItemAction(LootItem lootItem)
        {
            GetActionsClass.Class1612 @class = new GetActionsClass.Class1612();
            @class.rootItem = lootItem.Item;
            @class.owner = Singleton<GameWorld>.Instance.MainPlayer.gameObject.GetComponent<GamePlayerOwner>();
            @class.lootItemLastOwner = lootItem.LastOwner;
            @class.lootItemOwner = lootItem.ItemOwner;
            @class.controller = @class.owner.Player.InventoryController;

            
            return new CustomInteraction("Search", false, @class.method_3);
        }

        public static CustomInteraction GetDemolishItemAction(RemoteInteractable remoteInteractable)
        {
            return new CustomInteraction(
                "Reclaim",
                false,
                () =>
                {
                    DemolishItem(remoteInteractable);
                }
            );
        }

        public static void DemolishItem(RemoteInteractable remoteInteractable)
        {
            var session = ModSession.GetSession();
            var pair = session.GetPairOrNull(remoteInteractable);

            remoteInteractable.gameObject.transform.position = new Vector3(0, -9999, 0);
            pair.LootItem.gameObject.transform.position = pair.PlacementPosition;
            pair.LootItem.gameObject.transform.rotation = remoteInteractable.gameObject.transform.rotation;
            pair.Placed = false;
            session.PointsSpent -= GetItemCost(pair.LootItem.Item);

            DemolishPlayerFeedback(pair.LootItem.Item);
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
                    session.PointsSpent += GetItemCost(lootItem.Item);
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

        public static int GetItemCost(Item item, bool ignoreMinimumCostSetting = false)
        {
            int cost = 0;
 
            if (item is SearchableItemItemClass)
            {
                var searchableItem = item as SearchableItemItemClass;
                foreach (var grid in searchableItem.Grids)
                {
                    cost += grid.GridWidth * grid.GridHeight;
                }
            }
            else
            {
                var cellSizeStruct = item.CalculateCellSize();
                cost += cellSizeStruct.X * cellSizeStruct.Y;
            }
    
            if (ignoreMinimumCostSetting)
            {
                return cost;
            }
            else
            {
                return cost >= Settings.MinimumPlacementCost.Value
                    ? cost
                    : Settings.MinimumPlacementCost.Value;
            }
        }

        public static void PlacedPlayerFeedback(Item item)
        {
            var session = ModSession.GetSession();
            if (Settings.CostSystemEnabled.Value)
            {
                InteractionHelper.NotificationLong($"Placement cost: {PlacementController.GetItemCost(item)}, {Settings.GetAllottedPoints() - session.PointsSpent} out of {Settings.GetAllottedPoints()} points remaining");
            }
            Singleton<GUISounds>.Instance.PlayUISound(EUISoundType.MenuWeaponAssemble);
        }

        public static void DemolishPlayerFeedback(Item item)
        {
            var session = ModSession.GetSession();
            if (Settings.CostSystemEnabled.Value)
            {
                InteractionHelper.NotificationLong($"Points rufunded: {PlacementController.GetItemCost(item)}, {Settings.GetAllottedPoints() - session.PointsSpent} out of {Settings.GetAllottedPoints()} points remaining");
            }
            Singleton<GUISounds>.Instance.PlayUISound(EUISoundType.MenuWeaponDisassemble);
        }
    }
}
