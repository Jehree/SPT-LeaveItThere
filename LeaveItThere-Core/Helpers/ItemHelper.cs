using Comfort.Common;
using EFT;
using EFT.Interactive;
using EFT.InventoryLogic;
using LeaveItThere.Common;
using LeaveItThere.Components;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;

namespace LeaveItThere.Helpers
{
    internal static class ItemHelper
    {
        private static FieldInfo _idFieldInfo = null;

        public static LootItem GetLootItem(string itemId)
        {
            foreach (LootItem lootItem in ModSession.GetSession().GameWorld.LootItems.GetValuesEnumerator())
            {
                if (lootItem.ItemId == itemId) return lootItem;
            }
            return null;
        }

        public static void SpawnItem(Item item, Vector3 position, Quaternion rotation = default(Quaternion), Action<LootItem> callback = null)
        {
            StaticManager.BeginCoroutine(SpawnItemRoutine(item, position, rotation, callback));
        }

        public static void SpawnItem(string itemTpl, Vector3 position, Quaternion rotation = default(Quaternion), Action<LootItem> callback = null)
        {
            ItemFactoryClass itemFactory = Singleton<ItemFactoryClass>.Instance;
            var gameWorld = Singleton<GameWorld>.Instance;

            Item item = itemFactory.CreateItem(MongoID.Generate(), itemTpl, null);

            StaticManager.BeginCoroutine(SpawnItemRoutine(item, position, rotation, callback));
        }

        public static IEnumerator SpawnItemRoutine(Item item, Vector3 position, Quaternion rotation = default(Quaternion), Action<LootItem> callback = null)
        {
            if (!Singleton<GameWorld>.Instantiated)
            {
                throw new Exception("Tried to spawn an item while GameWorld was not instantiated!");
            }

            ItemFactoryClass itemFactory = Singleton<ItemFactoryClass>.Instance;
            var gameWorld = Singleton<GameWorld>.Instance;

            List<ResourceKey> collection = GetBundleResourceKeys(item);

            // this loads le bundles
            Task loadTask = Singleton<PoolManager>.Instance.LoadBundlesAndCreatePools(PoolManager.PoolsCategory.Raid, PoolManager.AssemblyType.Online, [.. collection], JobPriority.Immediate, null, default);
            while (!loadTask.IsCompleted)
            {
                yield return new WaitForEndOfFrame();
            }

            LootItem lootItem = gameWorld.SetupItem(item, gameWorld.MainPlayer, position, rotation);
            if (callback != null)
            {
                callback(lootItem);
            }
        }

        private static List<ResourceKey> GetBundleResourceKeys(Item item)
        {
            List<ResourceKey> collection = [];
            IEnumerable<Item> items = item.GetAllItems();
            foreach (Item subItem in items)
            {
                foreach (ResourceKey resourceKey in subItem.Template.AllResources)
                {
                    collection.Add(resourceKey);
                }
            }

            return collection;
        }

        public static void WriteItemToFile(string path, Item item)
        {
            string stringData = ItemToString(item);
            File.WriteAllText(path, stringData);
        }

        public static Item GetItemFromFile(string path)
        {
            if (!File.Exists(path))
            {
                Plugin.LogSource.LogWarning($"No Item file at path: {path}");
                return null;
            }
            string stringData = File.ReadAllText(path);
            return StringToItem(stringData);
        }

        public static byte[] ItemToBytes(Item item)
        {
            GClass1198 eftWriter = new();
            GClass1659 descriptor = GClass1685.SerializeItem(item, Singleton<GameWorld>.Instance.MainPlayer.SearchController);
            eftWriter.WriteEFTItemDescriptor(descriptor);
            return eftWriter.ToArray();
        }

        public static string ItemToString(Item item)
        {
            byte[] bytes = ItemToBytes(item);
            return Convert.ToBase64String(bytes);
        }

        public static Item BytesToItem(byte[] bytes)
        {
            GClass1193 eftReader = new(bytes);
            return GClass1685.DeserializeItem(eftReader.ReadEFTItemDescriptor(), Singleton<ItemFactoryClass>.Instance, []);
        }

        public static Item StringToItem(string base64String)
        {
            byte[] bytes = Convert.FromBase64String(base64String);
            return BytesToItem(bytes);
        }

        public static void MakeSearchableItemFullySearched(SearchableItemItemClass searchableItem)
        {
            var controller = Singleton<GameWorld>.Instance.MainPlayer.SearchController;
            controller.SetItemAsSearched(searchableItem);

            ForAllChildrenInItem(
                searchableItem,
                (Item item) =>
                {
                    controller.SetItemAsKnown(item);
                    if (item is SearchableItemItemClass)
                    {
                        controller.SetItemAsSearched(item as SearchableItemItemClass);
                    }
                }
            );
        }

        public static void ForAllChildrenInItem(Item parent, Action<Item> callable)
        {
            if (parent is not CompoundItem) return;
            var compoundParent = parent as CompoundItem;
            foreach (var grid in compoundParent.Grids)
            {
                IEnumerable<Item> children = grid.Items;

                foreach (Item child in children)
                {
                    callable(child);
                    ForAllChildrenInItem(child, callable);
                }
            }
        }

        public static object[] RemoveLostInsuredItemsByIds(object[] lostInsuredItems, List<string> idsToRemove)
        {
            if (_idFieldInfo == null && lostInsuredItems.Any())
            {
                // lazy initilization should be fine here.. I don't think it'll take THAT long to find it anyway
                _idFieldInfo = lostInsuredItems[0].GetType().GetField("_id");
            }

            List<string> placedItemIds = ModSession.GetSession().GetPlacedItemInstanceIds();
            List<object> editedLostInsuredItems = new();
            foreach (object item in lostInsuredItems)
            {
                string _id = _idFieldInfo.GetValue(item).ToString();
                if (!placedItemIds.Contains(_id)) continue;
                editedLostInsuredItems.Add(item);
            }

            return editedLostInsuredItems.ToArray();
        }

        public static void SetItemColor(Color color, GameObject gameObject)
        {
            var renderers = gameObject.GetComponentsInChildren<MeshRenderer>();
            foreach (var renderer in renderers)
            {
                var material = renderer.material;
                if (!material.HasProperty("_Color")) continue;
                renderer.material.color = color;
            }
        }

        public static int GetItemCost(Item item, bool ignoreMinimumCostSetting = false)
        {
            int cost = 0;

            if (item is SearchableItemItemClass)
            {
                // it happens to be the case that this does NOT include item cases. I'm not fixing it because I think it's cool heh heh
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

        public static void ForAllItemsUnderCost(int costAmount, Action<ItemRemotePair> callable)
        {
            var session = ModSession.GetSession();
            foreach (var pair in session.ItemRemotePairs)
            {
                if (pair.Placed == false) continue;
                if (ItemHelper.GetItemCost(pair.LootItem.Item, true) > costAmount) continue;
                callable(pair);
            }
        }

        public static bool ItemCanBePickedUp(Item item)
        {
            var session = ModSession.GetSession();
            InventoryController playerInventoryController = session.Player.InventoryController;
            InventoryEquipment playerEquipment = playerInventoryController.Inventory.Equipment;
            var pickedUpResult = InteractionsHandlerClass.QuickFindAppropriatePlace(item, playerInventoryController, playerEquipment.ToEnumerable<InventoryEquipment>(), InteractionsHandlerClass.EMoveItemOrder.PickUp, true);
            return pickedUpResult.Succeeded;
        }
    }
}
