using Comfort.Common;
using EFT;
using EFT.Interactive;
using EFT.InventoryLogic;
using LeaveItThere.Addon;
using LeaveItThere.Helpers;
using LeaveItThere.Models;
using LeaveItThere.ModSettings;
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
    public StateSynchronizerDatabase StateSynchronizerDatabase { get; internal set; }
    public Dictionary<string, FakeItem> FakeItems { get; private set; } = [];

    internal bool InteractionsAllowed { get; set; } = true;
    internal bool LootExperienceEnabled { get; set; } = true;

    private int _pointsSpent = 0;
    public int PointsSpent
    {
        get
        {
            return _pointsSpent;
        }
        private set
        {
            _pointsSpent = Mathf.Clamp(value, 0, Settings.AllottedPoints);
        }
    }

    internal void Awake()
    {
        GameWorld = Singleton<GameWorld>.Instance;
        Player = GameWorld.MainPlayer;
        GamePlayerOwner = Player.GetComponent<GamePlayerOwner>();
    }

    internal void InitOnGameStart()
    {
        ServerDataPack = LeaveItThereHelper.ServerRoute(Plugin.DataToClientURL, ServerDataPack.Request);
        Plugin.LogSource.LogError(ServerDataPack.StateSynchronizerDatabase == null);
        StateSynchronizerDatabase = ServerDataPack.StateSynchronizerDatabase ?? new();
        StateSynchronizerDatabase.Init();
        Plugin.LogSource.LogError(StateSynchronizerDatabase == null);
    }

    internal void AddFakeItem(FakeItem fakeItem)
    {
        FakeItems[fakeItem.ItemId] = fakeItem;
    }

    public FakeItem GetFakeItemOrNull(string itemId)
    {
        if (FakeItems.TryGetValue(itemId, out FakeItem fakeItem))
        {
            return fakeItem;
        }
        else
        {
            return null;
        }
    }

    internal FakeItem GetOrCreateFakeItem(string itemId)
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

    public bool PlacementIsAllowed(Item item)
    {
        if (Settings.CostSystemEnabled.Value)
        {
            return PointsSpent + LeaveItThereHelper.GetItemCost(item) <= Settings.AllottedPoints;
        }
        else
        {
            return true;
        }
    }

    internal void SpendPoints(int points)
    {
        PointsSpent += points;
    }

    internal void RefundPoints(int points)
    {
        PointsSpent -= points;
    }
}