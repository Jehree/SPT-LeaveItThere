using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using Newtonsoft.Json;
using PersistentCaches.Helpers;
using UnityEngine;

namespace PersistentCaches.Common
{
    internal class PersistentCacheData
    {
        public string OwnerProfileId { get; private set; }
        public Vector3 Location { get; private set; }
        public Quaternion Rotation { get; private set; }
        [JsonProperty ("CacheItemData")]
        private string _itemDataBase64;
        [JsonIgnore]
        private Item _item = null;
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
    
        public PersistentCacheData(Item item, Vector3 location, Quaternion rotation)
        {
            Location = location;
            Rotation = rotation;
            OwnerProfileId = Singleton<GameWorld>.Instance.MainPlayer.ProfileId;
            _itemDataBase64 = ItemHelper.ItemToString(item);
        }
    }
}
