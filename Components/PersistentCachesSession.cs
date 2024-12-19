using Comfort.Common;
using EFT;
using EFT.Interactive;
using PersistentCaches.Common;
using PersistentCaches.Helpers;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PersistentCaches.Components
{
    internal class PersistentCachesSession : MonoBehaviour
    {
        public List<CacheObjectPair> CacheObjectPairs { get; private set; } = new List<CacheObjectPair>();
        private static PersistentCachesSession _instance = null;

        private int _placedCachesCount = 0;
        public int PlacedChachesCount
        {
            get { return _placedCachesCount; }
            set { _placedCachesCount = Mathf.Clamp(value, 0, Settings.GetMaxCacheCount()); }
        }

        private PersistentCachesSession() {}

        public static PersistentCachesSession GetSession()
        {

            if (!Singleton<GameWorld>.Instantiated)
            {
                throw new Exception("Tried to get PersistentCachesSession when game world was not instantiated!");
            }
            if (_instance == null)
            {
                _instance = Singleton<GameWorld>.Instance.MainPlayer.gameObject.GetComponent<PersistentCachesSession>();
                return _instance;
            }
            return _instance;
        }

        public static void CreateNewSession()
        {
            _instance = Singleton<GameWorld>.Instance.MainPlayer.gameObject.AddComponent<PersistentCachesSession>();
        }

        public CacheObjectPair AddOrUpdatePair(ObservedLootItem lootItem, RemoteInteractableComponent remoteInteractableComponent, Vector3 cachePosition, bool placed)
        {
            var pair = GetPairOrNull(lootItem);
            if (pair == null)
            {
                CacheObjectPair newPair = new CacheObjectPair(lootItem, remoteInteractableComponent, cachePosition, placed);
                CacheObjectPairs.Add(newPair);
                return newPair;
            }
            else
            {
                pair.CachePosition = cachePosition;
                pair.Placed = placed;
                return pair;
            }
        }

        public CacheObjectPair GetPairOrNull(ObservedLootItem lootItem)
        {
            foreach (var pair in CacheObjectPairs)
            {
                if (pair.LootItem == lootItem) return pair;
            }

            return null;
        }

        public CacheObjectPair GetPairOrNull(RemoteInteractableComponent remoteInteractableComponent)
        {
            foreach (var pair in CacheObjectPairs)
            {
                if (pair.RemoteInteractableComponent == remoteInteractableComponent) return pair;
            }

            return null;
        }

        public bool CachePlacementIsAllowed()
        {
            return PlacedChachesCount < Settings.GetMaxCacheCount();
        }
    }
}
