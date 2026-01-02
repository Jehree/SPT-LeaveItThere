using Comfort.Common;
using EFT;
using EFT.Interactive;
using EFT.InventoryLogic;
using LeaveItThere.Helpers;
using LeaveItThere.Models;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LeaveItThere.Components;

public class RaidSession : MonoBehaviour
{
    private RaidSession() { }

    private static RaidSession _instance = null;
    public static RaidSession Instance
    {
        get
        {
            if (!Singleton<GameWorld>.Instantiated)
            {
                throw new Exception("Tried to get ModSession when game world was not instantiated!");
            }

            if (_instance == null)
            {
                _instance = Singleton<GameWorld>.Instance.MainPlayer.gameObject.GetOrAddComponent<RaidSession>();
            }

            return _instance;
        }
    }

    public GameWorld GameWorld { get; private set; }
    public Player Player { get; private set; }
    public GamePlayerOwner GamePlayerOwner { get; private set; }
    internal ServerDataPack ServerDataPack { get; set; }

    public Dictionary<string, FakeItem> FakeItems { get; private set; } = [];

    public bool InteractionsAllowed = true;

    internal void Awake()
    {
        GameWorld = Singleton<GameWorld>.Instance;
        Player = GameWorld.MainPlayer;
        GamePlayerOwner = Player.GetComponent<GamePlayerOwner>();
    }

    public void AddFakeItem(FakeItem fakeItem)
    {
        FakeItems[fakeItem.ItemId] = fakeItem;
    }

    public FakeItem GetFakeItemOrNull(string itemId)
    {
        return FakeItems.ContainsKey(itemId)
            ? FakeItems[itemId]
            : null;
    }

    public FakeItem GetOrCreateFakeItem(string itemId)
    {
        FakeItem fakeItem = GetFakeItemOrNull(itemId);

        // if it's null, it doesn't exist, so find the item for this item id and create a new FakeItem for it
        if (fakeItem == null)
        {
            ObservedLootItem lootItem = ItemHelper.GetLootItemInWorld(itemId) as ObservedLootItem;
            fakeItem = FakeItem.CreateNewFakeItem(lootItem);
        }

        return fakeItem;
    }

    public List<string> GetPlacedItemInstanceIds()
    {
        List<string> ids = [];

        foreach (FakeItem fakeItem in FakeItems.Values)
        {
            // sometimes things are null.. do this to avoid future null ref errors
            if (fakeItem == null || fakeItem.LootItem == null) continue;

            ids.Add(fakeItem.LootItem.Item.Id);
            ItemHelper.ForAllChildrenInItem
            (
                fakeItem.LootItem.Item,
                item => ids.Add(item.Id)
            );
        }

        return ids;
    }

    internal void DestroyAllFakeItems()
    {
        foreach (FakeItem fakeItem in FakeItems.Values)
        {
            Destroy(fakeItem.gameObject);
        }
    }
}