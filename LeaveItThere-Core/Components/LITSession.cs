using Comfort.Common;
using EFT;
using EFT.Interactive;
using EFT.InventoryLogic;
using LeaveItThere.Addon;
using LeaveItThere.Common;
using LeaveItThere.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LeaveItThere.Components
{
    public class LITSession : MonoBehaviour
    {
        /// <summary>
        /// Key = item template id, Value = the desired cost
        /// </summary>
        public static Dictionary<string, int> CostOverrides = [];

        public bool InteractionsAllowed { get; private set; } = true;
        public bool LootExperienceEnabled { get; private set; } = true;

        public GameWorld GameWorld { get; private set; }
        public Player Player { get; private set; }
        public GamePlayerOwner GamePlayerOwner { get; private set; }
        public Dictionary<string, FakeItem> FakeItems = [];

        private static LITSession _instance = null;
        public static LITSession Instance
        {
            get
            {
                if (!Singleton<GameWorld>.Instantiated)
                {
                    throw new Exception("Tried to get ModSession when game world was not instantiated!");
                }
                if (_instance == null)
                {
                    _instance = Singleton<GameWorld>.Instance.MainPlayer.gameObject.GetOrAddComponent<LITSession>();
                }
                return _instance;
            }
        }

        private int _pointsSpent = 0;
        public int PointsSpent
        {
            get { return _pointsSpent; }
            private set { _pointsSpent = Mathf.Clamp(value, 0, Settings.GetAllottedPoints()); }
        }

        public Dictionary<string, object> GlobalAddonData { get; private set; } = [];

        private LITSession() { }

        private void Awake()
        {
            GameWorld = Singleton<GameWorld>.Instance;
            Player = GameWorld.MainPlayer;
            GamePlayerOwner = Player.GetComponent<GamePlayerOwner>();
            SpawnAllPlacedItems();
        }

        internal static void CreateNewModSession()
        {
            _instance = Singleton<GameWorld>.Instance.MainPlayer.gameObject.GetOrAddComponent<LITSession>();
        }

        private static int _itemsSpawned;
        private static int _itemsToSpawn;
        private void SpawnAllPlacedItems()
        {
            PlacedItemDataPack dataPack = LITUtils.ServerRoute<PlacedItemDataPack>(Plugin.DataToClientURL, PlacedItemDataPack.Request);
            GlobalAddonData = dataPack.GlobalAddonData;

            _itemsSpawned = 0;
            _itemsToSpawn = dataPack.ItemTemplates.Count;

            if (_itemsToSpawn > 0)
            {
                LootExperienceEnabled = false;
            }

            for (int i = 0; i < dataPack.ItemTemplates.Count; i++)
            {
                PlacedItemData data = dataPack.ItemTemplates[i];
                if (data.Item == null) continue; // issue spawning item, error handling done by BytesToItem

                ItemHelper.SpawnItem(data.Item, new Vector3(0, -9999, 0), data.Rotation,
                (LootItem lootItem) =>
                {
                    if (lootItem.Item is SearchableItemItemClass)
                    {
                        ItemHelper.MakeSearchableItemFullySearched(lootItem.Item as SearchableItemItemClass);
                    }

                    FakeItem fakeItem = FakeItem.CreateNewFakeItem(lootItem as ObservedLootItem, data.AddonData);
                    fakeItem.PlaceAtPosition(data.Location, data.Rotation);

                    LITStaticEvents.InvokeOnPlacedItemSpawned(fakeItem);
                    fakeItem.InvokeOnFakeItemSpawned();

                    _itemsSpawned++;
                    if (_itemsSpawned >= _itemsToSpawn)
                    {
                        Instance.LootExperienceEnabled = true;
                        LITStaticEvents.InvokeOnLastPlacedItemSpawned(fakeItem);
                    }
                });
            }
        }

        internal void AddFakeItem(FakeItem fakeItem)
        {
            if (FakeItems.ContainsKey(fakeItem.ItemId)) return;
            FakeItems[fakeItem.ItemId] = fakeItem;
        }

        internal void RemoveFakeItem(FakeItem fakeItem)
        {
            if (!FakeItems.ContainsKey(fakeItem.ItemId)) return;
            FakeItems.Remove(fakeItem.ItemId);
        }

        public FakeItem GetFakeItemOrNull(string itemId)
        {
            if (!FakeItems.ContainsKey(itemId)) return null;
            return FakeItems[itemId];
        }

        public bool TryGetFakeItem(string itemId, out FakeItem fakeItem)
        {
            fakeItem = GetFakeItemOrNull(itemId);
            return fakeItem != null;
        }

        public bool PlacementIsAllowed(Item item)
        {
            if (Settings.CostSystemEnabled.Value)
            {
                return PointsSpent + ItemHelper.GetItemCost(item) <= Settings.GetAllottedPoints();
            }
            else
            {
                return true;
            }
        }

        internal void SpendPoints(int points)
        {
            PointsSpent += points;
        }

        internal void RefundPoints(int points)
        {
            PointsSpent -= points;
        }

        public List<string> GetPlacedItemInstanceIds()
        {
            List<string> ids = new();

            foreach (var kvp in FakeItems)
            {
                // sometimes things are null.. do this to avoid future null ref errors
                if (kvp.Value == null || kvp.Value.LootItem == null) continue;

                ids.Add(kvp.Value.LootItem.Item.Id);
                ItemHelper.ForAllChildrenInItem(kvp.Value.LootItem.Item,
                    (Item item) =>
                    {
                        ids.Add(item.Id);
                    }
                );
            }
            return ids;
        }

        internal void DestroyAllFakeItems()
        {
            foreach (var kvp in FakeItems)
            {
                Destroy(kvp.Value.gameObject);
            }
        }

        internal void SendPlacedItemDataToServer()
        {
            List<PlacedItemData> dataList = [];
            List<string> placedItemInstanceIds = [];
            foreach (var kvp in Instance.FakeItems)
            {
                FakeItem fakeItem = kvp.Value;
                dataList.Add(new PlacedItemData(fakeItem));
                placedItemInstanceIds.Add(fakeItem.ItemId);
            }
            PlacedItemDataPack dataPack = new(GlobalAddonData, dataList);
            LITUtils.ServerRoute(Plugin.DataToServerURL, dataPack);
        }

        public void SetInteractionsEnabled(bool enabled)
        {
            InteractionsAllowed = enabled;
        }

        /// <summary>
        /// Fetches Global Addon Data by it's key, returns null if none is found. T must be a class.
        /// </summary>
        /// <typeparam name="T">Must be a class. After Addon Data is fetched once, it can usually be modified directly without needing to use PutGlobalAddonData again since classes are stored by reference.</typeparam>
        /// <param name="key">Key that Addon Data was saved under.</param>
        public T GetGlobalAddonDataOrNull<T>(string key) where T : class
        {
            if (!GlobalAddonData.ContainsKey(key)) return null;

            if (GlobalAddonData[key] is not T)
            {
                object data = GlobalAddonData[key];
                T typedData = JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(data));
                GlobalAddonData[key] = typedData;
            }

            return (T)GlobalAddonData[key];
        }

        /// <summary>
        /// Saves Global Addon Data by a given key. Data will be saved to user/profiles/LeaveItThere-ItemData. Will be globally accessible when fetched, data is not associated with any specific item.
        /// </summary>
        /// <typeparam name="T">Must be a class.</typeparam>
        /// <param name="key">Key used to fetch the data later.</param>
        /// <param name="data"></param>
        public void PutGlobalAddonData<T>(string key, T data) where T : class
        {
            GlobalAddonData[key] = data;
        }
    }
}
