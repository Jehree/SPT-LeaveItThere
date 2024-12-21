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

namespace LeaveItThere.Helpers
{
    public static class ItemHelper
    {
        /* fuck this lol I'll just update the gclass numbers
        private static ConstructorInfo _writerConstructorMethodInfo;
        private static ConstructorInfo _readerConstructorMethodInfo;
        private static ConstructorInfo _descriptorConstructorMethodInfo;
        private static MethodInfo _serializeItemMethodInfo;
        private static MethodInfo _deserializeItemMethodInfo;

        public static void InitObfuscatedMethods()
        {

            Type writerClass = PatchConstants.EftTypes.Single(targetClass =>
                !targetClass.IsInterface &&
                !targetClass.IsNested &&
                targetClass.GetMethods().Any(method => method.Name == "WriteByte") &&
                targetClass.GetMethods().Any(method => method.Name == "Write") &&
                targetClass.GetFields().Any(field => field.Name == "MaxStringLength") &&
                targetClass.GetFields().Any(field => field.Name == "DefaultCapacity")
            );
            _writerConstructorMethodInfo = writerClass.GetConstructor(BindingFlags.Default, null, CallingConventions.Standard, new Type[0], null);

            Type readerClass = PatchConstants.EftTypes.Single(targetClass =>
                !targetClass.IsInterface &&
                !targetClass.IsNested &&
                targetClass.GetMethods().Any(method => method.Name == "ReadByte") &&
                targetClass.GetMethods().Any(method => method.Name == "ReadBytesSegment") &&
                targetClass.GetFields().Any(field => field.Name == "Position")
            );
            _readerConstructorMethodInfo = readerClass.GetConstructor(BindingFlags.Default, null, new Type[] { typeof(byte[])}, null);

            Type descriptorClass = PatchConstants.EftTypes.Single(targetClass =>
                !targetClass.IsInterface &&
                !targetClass.IsNested &&
                targetClass.GetMethods().Any(method => method.Name == "Deserialize" && method.GetParameters()[0].Name == "items" && method.GetParameters()[0].GetType() == typeof(Dictionary<MongoID, Item>)) &&
                targetClass.GetFields().Any(field => field.Name == "IsUnderBarrelDeviceActive")
            );
            _descriptorConstructorMethodInfo = descriptorClass.GetConstructor(BindingFlags.Default, null, new Type[0], null);

            Type itemSerizalizationClass = PatchConstants.EftTypes.Single(targetClass =>
                !targetClass.IsInterface &&
                !targetClass.IsNested &&
                targetClass.IsAbstract &&
                targetClass.GetMethods().Any(method => method.Name == "SerializeNestedItem" &&
                method.GetParameters().Length == 3 && 
                method.GetParameters()[0].GetType() == typeof(Item) &&
                method.IsStatic)
            );
            _serializeItemMethodInfo = itemSerizalizationClass.GetMethod("SerializeItem");
            _deserializeItemMethodInfo = itemSerizalizationClass.GetMethod("DeserializeItem");
        }
        */

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
    }
}
