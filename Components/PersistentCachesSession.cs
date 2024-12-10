using Comfort.Common;
using EFT;
using EFT.Interactive;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PersistentCaches.Components
{
    internal class PersistentCachesSession : MonoBehaviour
    {
        public List<CacheObjectPair> CacheObjectPairs { get; private set; } = new List<CacheObjectPair>();

        public static PersistentCachesSession GetSession()
        {
            if (!Singleton<GameWorld>.Instantiated)
            {
                throw new Exception("Tried to get PersistentCachesSession when game world was not instantiated!");
            }
            return Singleton<GameWorld>.Instance.MainPlayer.gameObject.GetComponent<PersistentCachesSession>();
        }

        public void AddPair(ObservedLootItem lootItem, RemoteInteractableComponent remoteInteractableComponent, Vector3 cachePosition)
        {
            foreach (var pair in CacheObjectPairs)
            {
                if (pair.LootItem == lootItem) return;
                if (pair.RemoteInteractableComponent == remoteInteractableComponent) return;
            }

            CacheObjectPairs.Add(new CacheObjectPair(lootItem, remoteInteractableComponent, cachePosition));
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

        public void DeletePair(CacheObjectPair pair)
        {
            CacheObjectPairs.Remove(pair);
        }

        public void DeletePair(ObservedLootItem lootItem)
        {
            foreach (var pair in CacheObjectPairs)
            {
                if (pair.LootItem == lootItem)
                {
                    CacheObjectPairs.Remove(pair);
                    return;
                }
            }
        }

        public void DeletePair(RemoteInteractableComponent remoteInteractableComponent)
        {
            foreach (var pair in CacheObjectPairs)
            {
                if (pair.RemoteInteractableComponent == remoteInteractableComponent)
                {
                    CacheObjectPairs.Remove(pair);
                    return;
                }
            }
        }
    }
}
