using EFT.Interactive;
using PersistentItemPlacement.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PersistentItemPlacement.Common
{
    internal class ItemRemotePair
    {
        public ObservedLootItem LootItem { get; private set; }
        public RemoteInteractable RemoteInteractableComponent { get; private set; }
        public Vector3 PlacementPosition { get; set; }
        public Quaternion PlacementRotation { get; set; }
        public bool Placed { get; set; }

        public ItemRemotePair(ObservedLootItem lootItem, RemoteInteractable remoteInteractableComponent, Vector3 placementPosition, Quaternion placementRotation, bool placed)
        {
            LootItem = lootItem;
            RemoteInteractableComponent = remoteInteractableComponent;
            PlacementPosition = placementPosition;
            PlacementRotation = placementRotation;
            Placed = placed;
        }
    }
}
