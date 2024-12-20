using Comfort.Common;
using EFT;
using EFT.Interactive;
using EFT.InventoryLogic;
using PersistentItemPlacement.Common;
using PersistentItemPlacement.Helpers;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PersistentItemPlacement.Components
{
    internal class ModSession : MonoBehaviour
    {
        public List<ItemRemotePair> ItemRemotePairs { get; private set; } = new List<ItemRemotePair>();
        private static ModSession _instance = null;

        private int _pointsSpent = 0;
        public int PointsSpent
        {
            get { return _pointsSpent; }
            set { _pointsSpent = Mathf.Clamp(value, 0, Settings.GetAllottedPoints()); }
        }

        private ModSession() {}

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

        public ItemRemotePair AddOrUpdatePair(ObservedLootItem lootItem, RemoteInteractable remoteInteractableComponent, Vector3 placementPosition, bool placed)
        {
            var pair = GetPairOrNull(lootItem);
            if (pair == null)
            {
                ItemRemotePair newPair = new ItemRemotePair(lootItem, remoteInteractableComponent, placementPosition, lootItem.gameObject.transform.rotation, placed);
                ItemRemotePairs.Add(newPair);
                return newPair;
            }
            else
            {
                pair.PlacementPosition = placementPosition;
                pair.Placed = placed;
                return pair;
            }
        }

        public ItemRemotePair GetPairOrNull(ObservedLootItem lootItem)
        {
            foreach (var pair in ItemRemotePairs)
            {
                if (pair.LootItem == lootItem) return pair;
            }

            return null;
        }

        public ItemRemotePair GetPairOrNull(RemoteInteractable remoteInteractableComponent)
        {
            foreach (var pair in ItemRemotePairs)
            {
                if (pair.RemoteInteractableComponent == remoteInteractableComponent) return pair;
            }

            return null;
        }

        public bool PlacementIsAllowed(Item item)
        {
            if (Settings.CostSystemEnabled.Value)
            {
                return PointsSpent + PlacementController.GetItemCost(item) <= Settings.GetAllottedPoints();
            }
            else
            {
                return true;
            }
        }
    }
}
