using EFT.Interactive;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PersistentCaches.Components
{
    internal class CacheObjectPair
    {
        public ObservedLootItem LootItem { get; private set; }
        public RemoteInteractableComponent RemoteInteractableComponent { get; private set; }
        public Vector3 CachePosition { get; private set; }

        public CacheObjectPair(ObservedLootItem lootItem, RemoteInteractableComponent remoteInteractableComponent, Vector3 cachePosition)
        {
            LootItem = lootItem;
            RemoteInteractableComponent = remoteInteractableComponent;
            CachePosition = cachePosition;
        }
    }
}
