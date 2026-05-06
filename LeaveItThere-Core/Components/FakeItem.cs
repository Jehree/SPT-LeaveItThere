using Comfort.Common;
using EFT.Interactive;
using EFT.UI;
using LeaveItThere.Addon;
using LeaveItThere.Common;
using LeaveItThere.Fika;
using LeaveItThere.Helpers;
using LeaveItThere.ModSettings;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace LeaveItThere.Components;

public class FakeItem : InteractableObject
{
    public string ItemId { get; private set; }
    public string TemplateId { get; private set; }
    public PhysicsMoveable Moveable { get; private set; }

    private ObservedLootItem _lootItem;
    public ObservedLootItem LootItem
    {
        get
        {
            // this is necessary because there are some cases where the Item Id goes null, like picking up a backpack straight from the ground into your backpack slot
            if (_lootItem.Item == null || _lootItem.Item.Id == null)
            {
                _lootItem = ItemHelper.GetLootItemInWorld(ItemId) as ObservedLootItem;
            }

            return _lootItem;
        }
    }

    public CustomInteractionContainer InteractionContainer { get; private set; }
    public StateSynchronizerDatabase StateSynchronizerDatabase { get; private set; }
    public FakeItemAddonFlags AddonFlags { get; private set; }

    private NavMeshObstacle _navMeshObstacle;

    internal static FakeItem CreateNewFakeItem(ObservedLootItem lootItem, StateSynchronizerDatabase stateSynchronizerDatabase = null)
    {
        GameObject gameObject = CreateFakeItemGameObject(lootItem.gameObject);
        FakeItem fakeItem = gameObject.AddComponent<FakeItem>();

        PhysicsMoveable moveable = gameObject.AddComponent<PhysicsMoveable>();
        fakeItem.Init(lootItem, moveable, stateSynchronizerDatabase);
        return fakeItem;
    }

    private static GameObject CreateFakeItemGameObject(GameObject lootItemObject)
    {
        GameObject gameObject = Instantiate(lootItemObject);

        ItemHelper.SetItemColor(Settings.PlacedItemTint.Value, gameObject);

        gameObject.transform.position = lootItemObject.transform.position;
        gameObject.transform.rotation = lootItemObject.transform.rotation;

        ObservedLootItem observedLootItemComponent = gameObject.GetComponent<ObservedLootItem>();
        if (observedLootItemComponent != null)
        {
            Destroy(observedLootItemComponent);
        }

        return gameObject;
    }

    private void Init(ObservedLootItem lootItem, PhysicsMoveable moveable, StateSynchronizerDatabase stateSynchronizerDatabase)
    {
        _lootItem = lootItem;
        ItemId = lootItem.ItemId;
        TemplateId = lootItem.TemplateId;
        Moveable = moveable;
        AddonFlags = new();
        StateSynchronizerDatabase = stateSynchronizerDatabase ?? new();
        StateSynchronizerDatabase.Init(this);

        RaidSession.Instance.AddFakeItem(this);

        AddNavMeshObstacle();
        InitializeInteractions();

        StaticEvents.InvokeFakeItemInitialized(this);

        SetPlayerAndBotCollisionEnabled(Settings.PlacedItemsHaveCollision.Value);

        if (AddonFlags.RemoveRootCollider || gameObject.name.Contains("LITRemoveRootCollider"))
        {
            GetComponents<BoxCollider>().ExecuteForEach(col => col.enabled = false);
        }
    }

    public void SetPlayerAndBotCollisionEnabled(bool enabled)
    {
        bool alwaysPhysical = AddonFlags.AlwaysPhysical || gameObject.name.Contains("LITAlwaysPhysical");
        bool itemIsTooSmall = LootItem.Item.Width * LootItem.Item.Height < Settings.MinimumSizeItemToGetCollision.Value;

        if (enabled && !alwaysPhysical && itemIsTooSmall) return;

        _navMeshObstacle.enabled = enabled;

        List<GameObject> descendants = LeaveItThereHelper.GetAllDescendants(gameObject);
        foreach (GameObject descendant in descendants)
        {
            if (!descendant.TryGetComponent<Collider>(out var _)) continue;
            if (descendant.name.Contains("LITKeepLayer")) continue;

            if (enabled)
            {
                descendant.layer = GetCollisionEnabledLayerNumber(descendant.name);
            }
            else
            {
                descendant.layer = LayerMask.NameToLayer("Loot");
            }
        }
    }

    public static int GetCollisionEnabledLayerNumber(string objectName)
    {
        if (!objectName.Contains("LITSetLayer")) return LayerMask.NameToLayer("Default");

        int startIndex = objectName.IndexOf("LITSetLayer") + "LITSetLayer".Length;
        int layerNumber = int.Parse(objectName.Substring(startIndex, 2));

        return layerNumber;
    }

    private void AddNavMeshObstacle()
    {
        _navMeshObstacle = gameObject.GetOrAddComponent<NavMeshObstacle>();

        BoxCollider collider = gameObject.GetComponent<BoxCollider>();
        _navMeshObstacle.shape = NavMeshObstacleShape.Box;
        _navMeshObstacle.center = collider.center;
        _navMeshObstacle.size = collider.size;
        _navMeshObstacle.carving = true;
        _navMeshObstacle.carveOnlyStationary = false;
    }

    private void InitializeInteractions()
    {
        InteractionContainer = new(this);

        if (LootItem.Item.IsContainer)
        {
            InteractionContainer.CustomInteractions.Add(new SearchInteraction(this));
        }

        InteractionContainer.CustomInteractions.Add(new ItemMover.EnterMoveModeInteraction(this));
        InteractionContainer.CustomInteractions.Add(new ReclaimInteraction(this));
    }

    public void Reclaim()
    {
        StaticEvents.InvokeItemReclaimed(this);

        SetRealItemLocation(gameObject.transform.position, gameObject.transform.rotation);
        Destroy(gameObject);

        RaidSession.Instance.RefundPoints(LeaveItThereHelper.GetItemCost(LootItem.Item));
    }

    internal void Place(Vector3 position, Quaternion rotation)
    {
        SetFakeItemLocation(position, rotation);
        SetRealItemLocation(new Vector3(0, -99999, 0));

        RaidSession.Instance.SpendPoints(LeaveItThereHelper.GetItemCost(LootItem.Item));
    }

    internal void Place()
    {
        Place(LootItem.gameObject.transform.position, LootItem.gameObject.transform.rotation);
    }

    public void SetFakeItemLocation(Vector3 position, Quaternion rotation)
    {
        LootItem.StopPhysics();
        gameObject.transform.position = position;
        gameObject.transform.rotation = rotation;

        StaticEvents.InvokeFakeItemMoved(this);
    }

    public void SetRealItemLocation(Vector3 position, Quaternion rotation = default)
    {
        LootItem.StopPhysics();
        LootItem.gameObject.transform.position = position;

        if (rotation != default)
        {
            LootItem.gameObject.transform.rotation = rotation;
        }
    }

    internal void PlacedPlayerFeedback()
    {
        if (Settings.CostSystemEnabled.Value)
        {
            InteractionHelper.NotificationLong($"Placement cost: {LeaveItThereHelper.GetItemCost(LootItem.Item)}, {Settings.AllottedPoints - RaidSession.Instance.PointsSpent} out of {Settings.AllottedPoints} points remaining");
        }

        Singleton<GUISounds>.Instance.PlayUISound(EUISoundType.MenuWeaponAssemble);
    }

    internal void ReclaimPlayerFeedback()
    {
        if (Settings.CostSystemEnabled.Value)
        {
            InteractionHelper.NotificationLong($"Points rufunded: {LeaveItThereHelper.GetItemCost(LootItem.Item)}, {Settings.AllottedPoints - RaidSession.Instance.PointsSpent} out of {Settings.AllottedPoints} points remaining");
        }

        Singleton<GUISounds>.Instance.PlayUISound(EUISoundType.MenuWeaponDisassemble);
    }

    public class SearchInteraction : CustomInteraction
    {
        private readonly GetActionsClass.Class1748 _searchClass;

        public SearchInteraction(FakeItem fakeItem)
        {
            _searchClass = new()
            {
                rootItem = fakeItem.LootItem.Item,
                owner = RaidSession.Instance.GamePlayerOwner,
                lootItemLastOwner = fakeItem.LootItem.LastOwner,
                lootItemOwner = fakeItem.LootItem.ItemOwner,
                controller = RaidSession.Instance.GamePlayerOwner.Player.InventoryController
            };
        }

        public override string Name => "Search";
        public override void OnInteract() => _searchClass.method_3();
    }

    public class PlaceItemInteraction(LootItem lootItem) : CustomInteraction
    {
        private readonly LootItem _lootItem = lootItem;
        public override string Name => "Place Item";
        public override bool Enabled => RaidSession.Instance.PlacementIsAllowed(_lootItem.Item);

        public override void OnInteract()
        {
            FakeItem fakeItem = CreateNewFakeItem(_lootItem as ObservedLootItem);
            fakeItem.Place();
            fakeItem.PlacedPlayerFeedback();
            FikaBridge.SendPlacedStateChangedPacket(fakeItem, true);
        }
    }

    public class ReclaimInteraction(FakeItem fakeItem) : CustomInteraction
    {
        public FakeItem FakeItem { get; set; } = fakeItem;
        public override string Name => FakeItem.AddonFlags.ReclaimInteractionDisabled
                                            ? $"Reclaim: {FakeItem.AddonFlags.ReclaimInteractionDisabledReason}"
                                            : "Reclaim";
        public override bool Enabled => !FakeItem.AddonFlags.ReclaimInteractionDisabled;
        public override bool AutoPromptRefresh => true;

        public override void OnInteract()
        {
            FakeItem.Reclaim();
            FakeItem.ReclaimPlayerFeedback();
            FikaBridge.SendPlacedStateChangedPacket(FakeItem, false);
        }
    }
}
