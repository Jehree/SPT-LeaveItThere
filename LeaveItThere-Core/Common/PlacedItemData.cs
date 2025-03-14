using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using LeaveItThere.Components;
using LeaveItThere.Fika;
using LeaveItThere.Helpers;
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

namespace LeaveItThere.Common
{
    internal class PlacedItemData
    {
        public Vector3 Location;
        public Quaternion Rotation;
        [JsonProperty("_itemDataBase64")]
        private string _itemDataBase64;
        [JsonIgnore]
        private Item _item = null;
        [JsonIgnore]
        public Item Item
        {
            get
            {
                if (_item == null)
                {
                    _item = ItemHelper.StringToItem(_itemDataBase64);
                }
                return _item;
            }
        }
        public Dictionary<string, object> AddonData = [];

        public PlacedItemData() { }
        public PlacedItemData(FakeItem fakeItem)
        {
            Location = fakeItem.gameObject.transform.position;
            Rotation = fakeItem.gameObject.transform.rotation;
            _itemDataBase64 = ItemHelper.ItemToString(fakeItem.LootItem.Item);
            AddonData = fakeItem.AddonData;
        }
    }

    internal class PlacedItemDataPack
    {
        public string ProfileId;
        public string MapId;
        public List<PlacedItemData> ItemTemplates;
        public Dictionary<string, object> GlobalAddonData = [];

        [JsonIgnore]
        public static PlacedItemDataPack Request
        {
            get
            {
                return new PlacedItemDataPack([]);
            }
        }

        public PlacedItemDataPack() { }
        public PlacedItemDataPack(Dictionary<string, object> globalAddonData, List<PlacedItemData> itemTemplates = null)
        {
            ProfileId = FikaBridge.GetRaidId();
            MapId = Singleton<GameWorld>.Instance.LocationId;
            ItemTemplates = [];
            GlobalAddonData = globalAddonData;
            if (itemTemplates != null)
            {
                ItemTemplates.AddRange(itemTemplates);
            }
        }
    }
}
