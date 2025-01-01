using Comfort.Common;
using EFT;
using EFT.Interactive;
using EFT.InventoryLogic;
using LeaveItThere.Common;
using LeaveItThere.Fika;
using LeaveItThere.Helpers;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LeaveItThere.Components
{
    internal class ModSession : MonoBehaviour
    {
        public bool InteractionsAllowed { get; private set; } = true;
        public bool CameraRotationLocked { get; set; } = false;

        public GameWorld GameWorld { get; private set; }
        public Player Player { get; private set; }
        public GamePlayerOwner GamePlayerOwner { get; private set; }

        public Dictionary<string, FakeItem> FakeItems { get; private set; } = new();

        private static ModSession _instance = null;
        public static ModSession Instance
        {
            get
            {
                if (!Singleton<GameWorld>.Instantiated)
                {
                    throw new Exception("Tried to get ModSession when game world was not instantiated!");
                }
                if (_instance == null)
                {
                    _instance = Singleton<GameWorld>.Instance.MainPlayer.gameObject.GetOrAddComponent<ModSession>();
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

        private ModSession() { }

        public void Awake()
        {
            GameWorld = Singleton<GameWorld>.Instance;
            Player = GameWorld.MainPlayer;
            GamePlayerOwner = Player.GetComponent<GamePlayerOwner>();
            OnRaidStart();
        }

        public static ModSession CreateNewModSession()
        {
            _instance = Singleton<GameWorld>.Instance.MainPlayer.gameObject.AddComponent<ModSession>();
            return _instance;
        }

        public void OnRaidStart()
        {
            string mapId = Singleton<GameWorld>.Instance.LocationId;
            PlacedItemDataPack dataPack = SPTServerHelper.ServerRoute<PlacedItemDataPack>(Plugin.DataToClientURL, new PlacedItemDataPack(FikaInterface.GetRaidId(), mapId));
            foreach (var data in dataPack.ItemTemplates)
            {
                ItemHelper.SpawnItem(data.Item, new Vector3(0, -9999, 0), data.Rotation,
                (LootItem lootItem) =>
                {
                    if (lootItem.Item is SearchableItemItemClass)
                    {
                        ItemHelper.MakeSearchableItemFullySearched(lootItem.Item as SearchableItemItemClass);
                    }

                    FakeItem fakeItem;
                    if (Instance.TryGetFakeItem(lootItem.ItemId, out FakeItem fakeItemFetch))
                    {
                        fakeItem = fakeItemFetch;
                    }
                    else
                    {
                        fakeItem = FakeItem.CreateNewRemoteInteractable(lootItem as ObservedLootItem);
                    }

                    fakeItem.PlaceAtLocation(data.Location, data.Rotation);
                });
            }
        }

        public void AddFakeItem(FakeItem fakeItem)
        {
            if (FakeItems.ContainsKey(fakeItem.ItemId))
            {
                throw new Exception("Tried to add FakeItem when one by it's key already existed!");
            }
            FakeItems[fakeItem.ItemId] = fakeItem;
        }

        public FakeItem GetFakeItemOrNull(string itemId)
        {
            if (!FakeItems.ContainsKey(itemId)) return null;
            return FakeItems[itemId];
        }

        public bool FakeItemExists(string itemId)
        {
            return FakeItems.ContainsKey(itemId);
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

        public void SpendPoints(int points)
        {
            PointsSpent += points;
        }

        public void RefundPoints(int points)
        {
            PointsSpent -= points;
        }

        public List<string> GetPlacedItemInstanceIds()
        {
            List<string> ids = new();

            foreach (var kvp in FakeItems)
            {
                if (kvp.Value.Placed == false) continue;

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

        public void DestroyAllRemoteObjects()
        {
            foreach (var kvp in FakeItems)
            {
                GameObject.Destroy(kvp.Value.gameObject);
            }
        }

        public void SendPlacedItemDataToServer()
        {
            var dataList = new List<PlacedItemData>();
            string mapId = Singleton<GameWorld>.Instance.LocationId;
            List<string> placedItemInstanceIds = new();
            foreach (var kvp in Instance.FakeItems)
            {
                FakeItem fakeItem = kvp.Value;
                if (fakeItem.Placed == false) continue;
                dataList.Add(new PlacedItemData(fakeItem.LootItem.Item, fakeItem.gameObject.transform.position, fakeItem.gameObject.transform.rotation));
                placedItemInstanceIds.Add(fakeItem.ItemId);
            }
            var dataPack = new PlacedItemDataPack(FikaInterface.GetRaidId(), mapId, dataList);
            SPTServerHelper.ServerRoute(Plugin.DataToServerURL, dataPack);
        }

        public void SetInteractionsEnabled(bool enabled)
        {
            InteractionsAllowed = enabled;
        }
    }
}
