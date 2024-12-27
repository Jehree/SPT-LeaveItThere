using EFT.Interactive;
using LeaveItThere.Components;
using LeaveItThere.Helpers;
using UnityEngine;

namespace LeaveItThere.Common
{
    internal class ItemRemotePair
    {
        public string ItemId { get; private set; }
        private ObservedLootItem _lootItem;
        public ObservedLootItem LootItem
        {
            get
            {
                if (_lootItem.Item == null || _lootItem.Item.Id == null)
                {
                    _lootItem = ItemHelper.GetLootItem(ItemId) as ObservedLootItem;
                }
                return _lootItem;
            }
        }
        public RemoteInteractable RemoteInteractable { get; private set; }
        public Vector3 PlacementPosition { get; set; }
        public Quaternion PlacementRotation { get; set; }
        public bool Placed { get; set; }

        public ItemRemotePair(ObservedLootItem lootItem, RemoteInteractable remoteInteractable, Vector3 placementPosition, Quaternion placementRotation, bool placed)
        {
            _lootItem = lootItem;
            ItemId = lootItem.ItemId;
            RemoteInteractable = remoteInteractable;
            PlacementPosition = placementPosition;
            PlacementRotation = placementRotation;
            Placed = placed;
        }
    }
}
