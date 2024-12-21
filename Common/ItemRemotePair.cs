using EFT.Interactive;
using LeaveItThere.Components;
using SPT.Reflection.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LeaveItThere.Common
{
    internal class ItemRemotePair
    {
        public ObservedLootItem LootItem { get; private set; }
        public RemoteInteractable RemoteInteractable { get; private set; }
        public Vector3 PlacementPosition { get; set; }
        public Quaternion PlacementRotation { get; set; }
        public bool Placed { get; set; }

        public ItemRemotePair(ObservedLootItem lootItem, RemoteInteractable remoteInteractable, Vector3 placementPosition, Quaternion placementRotation, bool placed)
        {
            LootItem = lootItem;
            RemoteInteractable = remoteInteractable;
            PlacementPosition = placementPosition;
            PlacementRotation = placementRotation;
            Placed = placed;
        }
    }
}
