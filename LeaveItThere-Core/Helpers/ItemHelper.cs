using Comfort.Common;
using EFT;
using EFT.Interactive;
using EFT.InventoryLogic;
using EFT.UI;
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
    public static class ItemHelper
    {
        private static FieldInfo _idFieldInfo = null;

        public static LootItem GetLootItem(string itemId)
        {
            foreach (LootItem lootItem in LITSession.Instance.GameWorld.LootItems.GetValuesEnumerator())
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
            GameWorld gameWorld = Singleton<GameWorld>.Instance;

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
            GameWorld gameWorld = Singleton<GameWorld>.Instance;

            List<ResourceKey> collection = GetBundleResourceKeys(item);

            // this loads le bundles
            Task loadTask = Singleton<PoolManager>.Instance.LoadBundlesAndCreatePools(PoolManager.PoolsCategory.Raid, PoolManager.AssemblyType.Online, [.. collection], JobPriority.Immediate, null, default);
            while (!loadTask.IsCompleted)
            {
                yield return new WaitForEndOfFrame();
            }

            LootItem lootItem = SetupItem(item, new Vector3(-99999, -99999, -99999), Quaternion.identity);

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
            try
            {
                GClass1193 eftReader = new(bytes);
                return GClass1685.DeserializeItem(eftReader.ReadEFTItemDescriptor(), Singleton<ItemFactoryClass>.Instance, []);
            }
            catch (Exception e)
            {
                string msg1 = "Failed to deserialize item from LeaveItThere-ItemData!";
                string msg2 = "This is usually caused by placing a modded item, updating the mod to a new version with changes made to that item,";
                string msg3 = "then trying to load into the map where it was placed. The item (and any contents if any) will be lost.";
                string msg4 = "Alt F4 and downgrade to an older version of culprit mod and unplace items before re-updating.";
                string msg5 = "Auto backups can also be used if needed, located in user/profiles/LeaveItThere-ItemData/[your_profile_id]/backups";
                string fullMsg = msg1 + msg2 + msg3 + msg4 + msg5;

                Plugin.LogSource.LogWarning(fullMsg);
                ConsoleScreen.LogWarning(msg5);
                ConsoleScreen.LogWarning(msg4);
                ConsoleScreen.LogWarning(msg3);
                ConsoleScreen.LogWarning(msg2);
                ConsoleScreen.LogWarning(msg1);
                InteractionHelper.NotificationLongWarning("Problem spawning item! Press ~ for more info!");
                Singleton<GUISounds>.Instance.PlayUISound(EUISoundType.ErrorMessage);

                Plugin.LogSource.LogError(e);
                return null;
            }
        }

        public static Item StringToItem(string base64String)
        {
            byte[] bytes = Convert.FromBase64String(base64String);
            return BytesToItem(bytes);
        }

        public static void MakeSearchableItemFullySearched(SearchableItemItemClass searchableItem)
        {
            IPlayerSearchController controller = Singleton<GameWorld>.Instance.MainPlayer.SearchController;
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
            CompoundItem compoundParent = parent as CompoundItem;
            foreach (StashGridClass grid in compoundParent.Grids)
            {
                IEnumerable<Item> children = grid.Items;

                foreach (Item child in children)
                {
                    callable(child);
                    ForAllChildrenInItem(child, callable);
                }
            }
            foreach (Slot slot in compoundParent.Slots)
            {
                IEnumerable<Item> children = slot.Items;

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

            List<object> editedLostInsuredItems = new();
            foreach (object item in lostInsuredItems)
            {
                string _id = _idFieldInfo.GetValue(item).ToString();
                if (idsToRemove.Contains(_id)) continue;
                editedLostInsuredItems.Add(item);
            }

            return editedLostInsuredItems.ToArray();
        }

        public static void SetItemColor(Color color, GameObject gameObject)
        {
            MeshRenderer[] renderers = gameObject.GetComponentsInChildren<MeshRenderer>();
            foreach (Renderer renderer in renderers)
            {
                Material material = renderer.material;
                if (!material.HasProperty("_Color")) continue;
                renderer.material.color = color;
            }
        }

        public static int GetItemCost(Item item, bool ignoreMinimumCostSetting = false)
        {
            if (LITSession.CostOverrides.ContainsKey(item.TemplateId))
            {
                return LITSession.CostOverrides[item.TemplateId];
            }

            int cost = 0;

            if (item is SearchableItemItemClass)
            {
                // it happens to be the case that this does NOT include item cases. I'm not fixing it because I think it's cool heh heh
                SearchableItemItemClass searchableItem = item as SearchableItemItemClass;
                foreach (StashGridClass grid in searchableItem.Grids)
                {
                    cost += grid.GridWidth * grid.GridHeight;
                }
            }
            else
            {
                XYCellSizeStruct cellSizeStruct = item.CalculateCellSize();
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

        public static void ForAllItemsUnderCost(int costAmount, Action<FakeItem> callable)
        {
            foreach (var kvp in LITSession.Instance.FakeItems)
            {
                FakeItem fakeItem = kvp.Value;
                if (GetItemCost(fakeItem.LootItem.Item, true) > costAmount) continue;
                callable(fakeItem);
            }
        }

        public static bool ItemCanBePickedUp(Item item)
        {
            LITSession session = LITSession.Instance;
            InventoryController playerInventoryController = session.Player.InventoryController;
            InventoryEquipment playerEquipment = playerInventoryController.Inventory.Equipment;
            var pickedUpResult = InteractionsHandlerClass.QuickFindAppropriatePlace(item, playerInventoryController, playerEquipment.ToEnumerable<InventoryEquipment>(), InteractionsHandlerClass.EMoveItemOrder.PickUp, true);
            return pickedUpResult.Succeeded;
        }

        /// <summary>
        /// Reimplementation of GameWorld.SetupItem that makes sure collisionDetectionMode is set correctly before rb is made to be kinematic to avoid unity warning spam.
        /// </summary>
        public static LootItem SetupItem(Item item, Vector3 position, Quaternion rotation)
        {
            new TraderControllerClass(item, item.Id, item.ShortName);

            GameObject gameObject = Singleton<PoolManager>.Instance.CreateLootPrefab(item, Player.GetVisibleToCamera(LITSession.Instance.Player));
            gameObject.SetActive(true);

            LootItem newLootItem = LITSession.Instance.GameWorld.CreateLootWithRigidbody(gameObject, item, item.ShortName, false, null, out BoxCollider boxCollider, true);

            Rigidbody rb = newLootItem.GetComponent<Rigidbody>();
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
            rb.isKinematic = true;

            newLootItem.transform.SetPositionAndRotation(position, rotation);
            //newLootItem.LastOwner = LITSession.Instance.Player;

            return newLootItem;
        }
    }
}
