using EFT.InventoryLogic;
using EFT;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Comfort.Common;
using System.Collections;
using Newtonsoft.Json;
using System.Text;
using System.IO;
using System.Reflection;
using EFT.UI.DragAndDrop;
using System.Net.Http;

namespace PersistentCaches.Helpers
{
    public static class ItemHelper
    {
        public static void SpawnItem(Item item, Vector3 position, Quaternion rotation = default(Quaternion))
        {
            StaticManager.BeginCoroutine(SpawnItemRoutine(item, position, rotation));
        }

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

            List<ResourceKey> collection = GetBundleResourceKeys(item);

            // this loads le bundles
            Task loadTask = Singleton<PoolManager>.Instance.LoadBundlesAndCreatePools(PoolManager.PoolsCategory.Raid, PoolManager.AssemblyType.Online, [.. collection], JobPriority.Immediate, null, default);
            while (!loadTask.IsCompleted)
            {
                yield return new WaitForEndOfFrame();
            }

            gameWorld.SetupItem(item, gameWorld.MainPlayer, position, rotation);
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
            GClass1659 descriptor = GClass1685.SerializeItem(item, GClass1971.Instance);
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
    }
}
