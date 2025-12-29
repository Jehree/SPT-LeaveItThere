using Comfort.Common;
using EFT.Interactive;
using EFT.UI;
using LeaveItThere.Common;
using LeaveItThere.Fika;
using LeaveItThere.Helpers;
using LeaveItThere.ModSettings;
using UnityEngine;
using UnityEngine.AI;

namespace LeaveItThere.Components;

public class FakeItem : InteractableObject
{
    public string ItemId { get; private set; }
    public string TemplateId { get; private set; }

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

    private NavMeshObstacle _navMeshObstacle;

    internal static FakeItem CreateNewFakeItem(ObservedLootItem lootItem)
    {
        GameObject gameObject = CreateFakeItemGameObjectFromLootItemObject(lootItem.gameObject);
        FakeItem fakeItem = gameObject.AddComponent<FakeItem>();
        fakeItem.Init(lootItem);
        return fakeItem;
    }

    private static GameObject CreateFakeItemGameObjectFromLootItemObject(GameObject lootItemObject)
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

    private void Init(ObservedLootItem lootItem)
    {
        _lootItem = lootItem;
        ItemId = lootItem.ItemId;
        TemplateId = lootItem.TemplateId;

        RaidSession.Instance.AddFakeItem(this);

        AddNavMeshObstacle();
        InitializeInteractions();

        // TODO: add collision functionality
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

        // TODO: add move mode interaction

        InteractionContainer.CustomInteractions.Add(new ReclaimInteraction(this));
    }

    public void Reclaim()
    {
        SetRealItemLocation(gameObject.transform.position, gameObject.transform.rotation);
        Destroy(gameObject);
    }

    public void Place(Vector3 position, Quaternion rotation)
    {
        LootItem.StopPhysics();
        SetFakeItemLocation(position, rotation);
        LootItem.gameObject.transform.position = new Vector3(0, -99999, 0);
    }

    public void PlaceAtLootItem()
    {
        Place(LootItem.gameObject.transform.position, LootItem.gameObject.transform.rotation);
    }

    private void SetFakeItemLocation(Vector3 position, Quaternion rotation)
    {
        gameObject.transform.position = position;
        gameObject.transform.rotation = rotation;
    }

    private void SetRealItemLocation(Vector3 position, Quaternion rotation)
    {
        LootItem.gameObject.transform.position = position;
        LootItem.gameObject.transform.rotation = rotation;
    }

    public void PlacedPlayerFeedback()
    {
        Singleton<GUISounds>.Instance.PlayUISound(EUISoundType.MenuWeaponAssemble);
    }

    public void ReclaimPlayerFeedback()
    {
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
        private LootItem _lootItem = lootItem;
        public override string Name => "Place Item";
        //public override bool Enabled => LITSession.Instance.PlacementIsAllowed(_lootItem.Item);

        public override void OnInteract()
        {
            FakeItem fakeItem = CreateNewFakeItem(_lootItem as ObservedLootItem);
            fakeItem.PlaceAtLootItem();
            fakeItem.PlacedPlayerFeedback();
            FikaBridge.SendPlacedStateChangedPacket(fakeItem, true);
        }
    }

    public class ReclaimInteraction(FakeItem fakeItem) : CustomInteraction
    {
        public FakeItem FakeItem { get; set; } = fakeItem;
        public override string Name => "Reclaim";
        public override bool Enabled => true;
        public override bool AutoPromptRefresh => true;

        public override void OnInteract()
        {
            //FikaBridge.SendPlacedStateChangedPacket(FakeItem, false);
            FakeItem.Reclaim();
            FakeItem.ReclaimPlayerFeedback();
            FikaBridge.SendPlacedStateChangedPacket(FakeItem, false);
        }
    }
}
