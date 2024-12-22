using EFT.InventoryLogic;
using EFT;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Comfort.Common;
using System.Collections;
using System.IO;
using EFT.Interactive;
using System.Reflection;
using SPT.Reflection.Utils;
using System.Linq;
using LeaveItThere.Components;

namespace LeaveItThere.Helpers
{
    public static class ItemHelper
    {
        private static FieldInfo _idFieldInfo = null;

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
    }
}
