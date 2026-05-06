using EFT.InventoryLogic;
using LeaveItThere.Addon;
using LeaveItThere.Components;
using LeaveItThere.Helpers;
using Newtonsoft.Json;
using UnityEngine;

namespace LeaveItThere.Models;

public class PlacedItemData
{
    [JsonProperty("_itemDataBase64")]
    private readonly string _itemDataBase64;
    public Vector3 Location;
    public Quaternion Rotation;
    public StateSynchronizerDatabase StateSynchronizerDatabase;

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
    public PlacedItemData(FakeItem fakeItem)
    {
        Location = fakeItem.gameObject.transform.position;
        Rotation = fakeItem.gameObject.transform.rotation;
        _itemDataBase64 = ItemHelper.ItemToString(fakeItem.LootItem.Item);
        fakeItem.StateSynchronizerDatabase.UpdateRawDatabase();
        StateSynchronizerDatabase = fakeItem.StateSynchronizerDatabase;
    }
}
