using EFT.InventoryLogic;
using EFT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Comfort.Common;
using EFT.Interactive;
using System.Collections;

namespace PersistentCaches.Helpers
{
    public static class ItemHelper
    {
        public static void SpawnItem(string itemTpl, Vector3 position, Quaternion rotation = default(Quaternion))
        {
            ItemFactoryClass itemFactory = Singleton<ItemFactoryClass>.Instance;
            var gameWorld = Singleton<GameWorld>.Instance;

            Item item = itemFactory.CreateItem(MongoID.Generate(), itemTpl, null);

            StaticManager.BeginCoroutine(SpawnItemRoutine(item, position, rotation));
        }

        public static IEnumerator SpawnItemRoutine(Item item, Vector3 position, Quaternion rotation = default(Quaternion))
        {
            if (!Singleton<GameWorld>.Instantiated)
            {
                throw new Exception("Tried to spawn an item while GameWorld was not instantiated!");
            }

            ItemFactoryClass itemFactory = Singleton<ItemFactoryClass>.Instance;
            var gameWorld = Singleton<GameWorld>.Instance;

            List<ResourceKey> collection = LoadItemBundles(item);

            Task loadTask = Singleton<PoolManager>.Instance.LoadBundlesAndCreatePools(PoolManager.PoolsCategory.Raid, PoolManager.AssemblyType.Online, [.. collection], JobPriority.Immediate, null, default);
            while (!loadTask.IsCompleted)
            {
                yield return new WaitForEndOfFrame();
            }

            gameWorld.SetupItem(item, gameWorld.MainPlayer, position, rotation);
        }

        public static List<ResourceKey> LoadItemBundles(Item item)
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
    }
}
