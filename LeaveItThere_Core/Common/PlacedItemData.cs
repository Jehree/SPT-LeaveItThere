using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using Newtonsoft.Json;
using LeaveItThere.Helpers;
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
        
        public PlacedItemData() { }
        public PlacedItemData(Item item, Vector3 location, Quaternion rotation)
        {
            Location = location;
            Rotation = rotation;
            _itemDataBase64 = ItemHelper.ItemToString(item);
        }
    }

    internal class PlacedItemDataPack
    {
        public string MapId;
        public List<PlacedItemData> ItemTemplates;

        public PlacedItemDataPack() { }
        public PlacedItemDataPack(string mapId, List<PlacedItemData> itemTemplates = null)
        {
            MapId = mapId;
            ItemTemplates = new List<PlacedItemData>();
            if (itemTemplates != null)
            {
                ItemTemplates.AddRange(itemTemplates);
            }
        }
    }
}
