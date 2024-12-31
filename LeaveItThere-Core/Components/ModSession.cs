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
        public GameWorld GameWorld { get; private set; }
        public Player Player { get; private set; }
        public GamePlayerOwner GamePlayerOwner { get; private set; }

        public List<ItemRemotePair> ItemRemotePairs { get; private set; } = new List<ItemRemotePair>();
        private static ModSession _instance = null;

        private int _pointsSpent = 0;
        public int PointsSpent
        {
            get { return _pointsSpent; }
            set { _pointsSpent = Mathf.Clamp(value, 0, Settings.GetAllottedPoints()); }
        }

        private ModSession() { }

        public void Awake()
        {
            GameWorld = Singleton<GameWorld>.Instance;
            Player = GameWorld.MainPlayer;
            GamePlayerOwner = Player.GetComponent<GamePlayerOwner>();
            OnRaidStart();
        }

        public static void OnRaidStart()
        {
            string mapId = Singleton<GameWorld>.Instance.LocationId;
            PlacedItemDataPack dataPack = SPTServerHelper.ServerRoute<PlacedItemDataPack>(ItemPlacer.DataToClientURL, new PlacedItemDataPack(FikaInterface.GetRaidId(), mapId));
            foreach (var data in dataPack.ItemTemplates)
            {
                ItemHelper.SpawnItem(data.Item, new Vector3(0, -9999, 0), data.Rotation,
                (LootItem lootItem) =>
                {
                    ItemPlacer.PlaceItem(lootItem as ObservedLootItem, data.Location, data.Rotation);
                    
                    if (lootItem.Item is SearchableItemItemClass)
                    {
                        ItemHelper.MakeSearchableItemFullySearched(lootItem.Item as SearchableItemItemClass);
                    }
                });
            }
        }

        public static ModSession GetSession()
        {

            if (!Singleton<GameWorld>.Instantiated)
            {
                throw new Exception("Tried to get ModSession when game world was not instantiated!");
            }
            if (_instance == null)
            {
                _instance = Singleton<GameWorld>.Instance.MainPlayer.gameObject.GetComponent<ModSession>();
                return _instance;
            }
            return _instance;
        }

        public static void CreateNewSession()
        {
            _instance = Singleton<GameWorld>.Instance.MainPlayer.gameObject.AddComponent<ModSession>();
        }

        public ItemRemotePair AddPair(ObservedLootItem lootItem, RemoteInteractable remoteInteractable, Vector3 placementPosition, Quaternion placementRotation, bool placed)
        {
            ItemRemotePair newPair = new ItemRemotePair(lootItem, remoteInteractable, placementPosition, placementRotation, placed);
            ItemRemotePairs.Add(newPair);
            newPair.RemoteInteractable.Init(lootItem);
            return newPair;
        }

        public ItemRemotePair UpdatePair(ItemRemotePair pair, Vector3 placementPosition, Quaternion placementRotation, bool placed)
        {
            //pair.PlacementPosition = placementPosition;
            //pair.PlacementRotation = placementRotation;
            pair.Placed = placed;
            return pair;
        }

        public ItemRemotePair GetPairOrNull(ObservedLootItem lootItem)
        {
            foreach (var pair in ItemRemotePairs)
            {
                if (pair.LootItem == lootItem) return pair;
            }
            return null;
        }

        public ItemRemotePair GetPairOrNull(RemoteInteractable remoteInteractable)
        {
            foreach (var pair in ItemRemotePairs)
            {
                if (pair.RemoteInteractable == remoteInteractable) return pair;
            }
            return null;
        }

        public ItemRemotePair GetPairOrNull(string itemId)
        {
            foreach (var pair in ItemRemotePairs)
            {
                if (pair.ItemId == itemId) return pair;
            }

            return null;
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

        public List<string> GetPlacedItemInstanceIds()
        {
            List<string> ids = new();

            foreach (var pair in ItemRemotePairs)
            {
                if (pair.Placed == false) continue;

                ids.Add(pair.LootItem.Item.Id);
                ItemHelper.ForAllChildrenInItem(pair.LootItem.Item,
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
            foreach (var pair in ItemRemotePairs)
            {
                GameObject.Destroy(pair.RemoteInteractable.gameObject);
            }
        }
    }
}
