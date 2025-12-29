using Comfort.Common;
using EFT;
using EFT.Interactive;
using EFT.InventoryLogic;
using LeaveItThere.Components;
using LeaveItThere.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace LeaveItThere.Helpers;

public static class ItemSpawner
{
    private static int _itemsSpawned;
    private static int _itemsToSpawn;
    private static Vector3 _itemSpawnPosition = new(0, -9999, 0);

    internal static void SpawnAllPlacedItems()
    {
        ServerDataPack dataPack = LITUtils.ServerRoute(Plugin.DataToClientURL, ServerDataPack.Request);

        _itemsSpawned = 0;
        _itemsToSpawn = dataPack.ItemTemplates.Count;

        foreach (PlacedItemData placedItemData in dataPack.ItemTemplates)
        {
            if (placedItemData.Item == null) continue; // issue spawning item, error handling done by BytesToItem

            SpawnItem(placedItemData, ItemSpawnedCallback);
        }
    }

    private readonly static Action<LootItem, PlacedItemData> ItemSpawnedCallback = ItemSpawned;
    private static void ItemSpawned(LootItem lootItem, PlacedItemData placedItemData)
    {
        if (lootItem.Item is SearchableItemItemClass)
        {
            ItemHelper.MakeSearchableItemFullySearched(lootItem.Item as SearchableItemItemClass);
        }

        FakeItem fakeItem = FakeItem.CreateNewFakeItem(lootItem as ObservedLootItem);
        fakeItem.Place(placedItemData.Location, placedItemData.Rotation);

        _itemsSpawned++;
        if (_itemsSpawned >= _itemsToSpawn)
        {
            // reenable loot experience
            // invoke finished spawning events
        }
    }

    public static void SpawnItem(
        Item item,
        Vector3 position,
        Quaternion rotation = default,
        Action<LootItem, PlacedItemData> callback = null)
    {
        StaticManager.BeginCoroutine(SpawnItemRoutine(item, position, rotation, callback, null));
    }

    public static void SpawnItem(
    PlacedItemData placedItemData,
    Action<LootItem, PlacedItemData> callback)
    {
        StaticManager.BeginCoroutine(SpawnItemRoutine(placedItemData.Item, _itemSpawnPosition, placedItemData.Rotation, callback, placedItemData));
    }

    private static IEnumerator SpawnItemRoutine(
        Item item,
        Vector3 position,
        Quaternion rotation = default,
        Action<LootItem, PlacedItemData> callback = null,
        PlacedItemData placedItemData = null)
    {
        if (!Singleton<GameWorld>.Instantiated)
        {
            throw new Exception("Tried to spawn an item while GameWorld was not instantiated!");
        }

        List<ResourceKey> collection = GetBundleResourceKeys(item);

        // this loads the item's bundles
        Task loadTask = Singleton<PoolManagerClass>.Instance.LoadBundlesAndCreatePools(
            PoolManagerClass.PoolsCategory.Raid,
            PoolManagerClass.AssemblyType.Online,
            collection,
            JobPriorityClass.Immediate,
            null,
            default);

        // loading is async
        while (!loadTask.IsCompleted)
        {
            yield return new WaitForEndOfFrame();
        }

        LootItem lootItem = SetupItem(item, new Vector3(-99999, -99999, -99999), Quaternion.identity);

        callback?.Invoke(lootItem, placedItemData);
    }

    private static List<ResourceKey> GetBundleResourceKeys(Item item)
    {
        List<ResourceKey> collection = [];
        IEnumerable<Item> items = item.GetAllItems();

        foreach (Item subItem in items)
        {
            foreach (ResourceKey resourceKey in subItem.Template.AllResources)
            {
                collection.Add(resourceKey);
            }
        }

        return collection;
    }

    /// <summary>
    /// Reimplementation of GameWorld.SetupItem that makes sure collisionDetectionMode is set correctly before rb is made to be kinematic to avoid unity warning spam.
    /// </summary>
    public static LootItem SetupItem(Item item, Vector3 position, Quaternion rotation)
    {
        new TraderControllerClass(item, item.Id, item.ShortName);

        GameObject gameObject = Singleton<PoolManagerClass>.Instance.CreateLootPrefab(item, Player.GetVisibleToCamera(RaidSession.Instance.Player));
        gameObject.SetActive(true);

        LootItem newLootItem = RaidSession.Instance.GameWorld.CreateLootWithRigidbody(gameObject, item, item.ShortName, false, null, out BoxCollider boxCollider, true);

        Rigidbody rb = newLootItem.GetComponent<Rigidbody>();
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        rb.isKinematic = true;

        newLootItem.transform.SetPositionAndRotation(position, rotation);

        return newLootItem;
    }
}
