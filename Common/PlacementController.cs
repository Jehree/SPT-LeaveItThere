using EFT.Interactive;
using InteractableInteractionsAPI.Common;
using PersistentItemPlacement.Components;
using UnityEngine;
using Comfort.Common;
using EFT;
using PersistentItemPlacement.Helpers;
using Diz.LanguageExtensions;
using EFT.InventoryLogic;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using EFT.UI;

namespace PersistentItemPlacement.Common
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

        public static void OnRaidEnd()
        {
            var dataList = new List<PlacedItemData>();
            string mapId = Singleton<GameWorld>.Instance.LocationId;
            foreach (var pair in ModSession.GetSession().ItemRemotePairs)
            {
                if (pair.Placed == false) continue;
                dataList.Add(new PlacedItemData(pair.LootItem.Item, pair.PlacementPosition, pair.PlacementRotation));
            }
            var dataPack = new PlacedItemDataPack(mapId, dataList);
            SPTServerHelper.ServerRoute(DataToServerURL, dataPack);
        }

        public static RemoteInteractableComponent GetOrCreateRemoteAccessComponent(Vector3 position, LootItem lootItem, ItemRemotePair pair = null)
        {
            if (pair == null)
            {
                return CreateNewRemoteItemAccessObject(position, lootItem.gameObject.transform.rotation, lootItem as ObservedLootItem);
            }
            else
            {
                pair.RemoteInteractableComponent.gameObject.transform.position = position;
                pair.RemoteInteractableComponent.gameObject.transform.rotation = lootItem.gameObject.transform.rotation;
                return pair.RemoteInteractableComponent;
            }
        }

        private static RemoteInteractableComponent CreateNewRemoteItemAccessObject(Vector3 position, Quaternion rotation, ObservedLootItem lootItem)
        {
            GameObject remoteAccessObj = GameObject.Instantiate(lootItem.gameObject);

            ObservedLootItem componentWhoLivedComeToDie = remoteAccessObj.GetComponent<ObservedLootItem>();
            if (componentWhoLivedComeToDie != null)
            {
                GameObject.Destroy(componentWhoLivedComeToDie); //hehe
            }

            MeshRenderer[] renderers = remoteAccessObj.GetComponentsInChildren<MeshRenderer>();

            foreach (var renderer in renderers)
            {
                var material = renderer.material;
                if (!material.HasProperty("_Color")) continue;

                var originalColor = renderer.material.color;
                renderer.material.color = Color.yellow;
            }

            RemoteInteractableComponent remoteInteractableComponent = remoteAccessObj.AddComponent<RemoteInteractableComponent>();
            if (lootItem.Item.IsContainer)
            {
                remoteInteractableComponent.Actions.Add(GetRemoteOpenItemAction(lootItem));
            }
            remoteInteractableComponent.Actions.Add(GetDemolishItemAction(remoteInteractableComponent));

            remoteAccessObj.transform.position = position;
            remoteAccessObj.transform.rotation = rotation;
            remoteAccessObj.transform.localScale = remoteAccessObj.transform.localScale * 0.95f;

            return remoteInteractableComponent;
        }

        public static CustomInteractionAction GetRemoteOpenItemAction(LootItem lootItem)
        {
            GetActionsClass.Class1612 @class = new GetActionsClass.Class1612();
            @class.rootItem = lootItem.Item;
            @class.owner = Singleton<GameWorld>.Instance.MainPlayer.gameObject.GetComponent<GamePlayerOwner>();
            @class.lootItemLastOwner = lootItem.LastOwner;
            @class.lootItemOwner = lootItem.ItemOwner;
            @class.controller = @class.owner.Player.InventoryController;

            
            return new CustomInteractionAction("Search", false, @class.method_3);
        }

        public static CustomInteractionAction GetDemolishItemAction(RemoteInteractableComponent remoteInteractableComponent)
        {
            return new CustomInteractionAction(
                "Reclaim",
                false,
                () =>
                {
                    var session = ModSession.GetSession();
                    var pair = session.GetPairOrNull(remoteInteractableComponent);

                    remoteInteractableComponent.gameObject.transform.position = new Vector3(0, -9999, 0);
                    pair.LootItem.gameObject.transform.position = pair.PlacementPosition;
                    pair.LootItem.gameObject.transform.rotation = remoteInteractableComponent.gameObject.transform.rotation;
                    pair.Placed = false;
                    session.PointsSpent -= GetItemCost(pair.LootItem.Item);

                    DemolishPlayerFeedback(pair.LootItem.Item);
                }
            );
        }

        public static CustomInteractionAction GetPlaceItemAction(LootItem lootItem)
        {
            var session = ModSession.GetSession();
            bool allowed = session.PlacementIsAllowed(lootItem.Item);
            return new CustomInteractionAction(
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

            RemoteInteractableComponent remoteInteractableComponent = GetOrCreateRemoteAccessComponent(location, lootItem, session.GetPairOrNull(lootItem));
            session.AddOrUpdatePair(lootItem, remoteInteractableComponent, location, true);
        }

        public static int GetItemCost(Item item)
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
    
            return cost >= Settings.MinimumPlacementCost.Value 
                ? cost 
                : Settings.MinimumPlacementCost.Value;
        }

        public static void PlacedPlayerFeedback(Item item)
        {
            var session = ModSession.GetSession();
            InteractionHelper.NotificationLong($"Placement cost: {PlacementController.GetItemCost(item)}, {Settings.GetAllottedPoints() - session.PointsSpent}/{Settings.GetAllottedPoints()} points remaining");
            Singleton<GUISounds>.Instance.PlayUISound(EUISoundType.MenuWeaponAssemble);
        }

        public static void DemolishPlayerFeedback(Item item)
        {
            var session = ModSession.GetSession();
            InteractionHelper.NotificationLong($"Points rufunded: {PlacementController.GetItemCost(item)}, {Settings.GetAllottedPoints() - session.PointsSpent}/{Settings.GetAllottedPoints()} points remaining");
            Singleton<GUISounds>.Instance.PlayUISound(EUISoundType.MenuWeaponDisassemble);
        }
    }
}
