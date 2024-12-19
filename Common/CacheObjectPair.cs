using EFT.Interactive;
using PersistentCaches.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PersistentCaches.Common
{
    internal class CacheObjectPair
    {
        public ObservedLootItem LootItem { get; private set; }
        public RemoteInteractableComponent RemoteInteractableComponent { get; private set; }
        public Vector3 CachePosition { get; set; }
        public bool Placed { get; set; }

        public CacheObjectPair(ObservedLootItem lootItem, RemoteInteractableComponent remoteInteractableComponent, Vector3 cachePosition, bool placed)
        {
            LootItem = lootItem;
            RemoteInteractableComponent = remoteInteractableComponent;
            CachePosition = cachePosition;
            Placed = placed;
        }
    }
}
