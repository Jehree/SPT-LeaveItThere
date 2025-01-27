using Comfort.Common;
using EFT;
using EFT.Interactive;
using EFT.UI;
using LeaveItThere.Common;
using LeaveItThere.Fika;
using LeaveItThere.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace LeaveItThere.Components
{
    public class AddonFlags
    {
        /// <summary>
        /// When true, the Move interaction is disabled.
        /// </summary>
        public bool MoveModeDisabled = false;
        /// <summary>
        /// When true, the Reclaim interaction is disabled.
        /// </summary>
        public bool ReclaimInteractionDisabled = false;
        /// <summary>
        /// Set to true to make this FakeItem ignore size restrictions and always be have player collision / block bot pathing.
        /// </summary>
        public bool IsPhysicalRegardlessOfSize = false;
        /// <summary>
        /// When true, the root BoxCollider component that is added to LootItems will be removed. Useful for when a custom item has a child object defining a custom collider to be used for interactions.
        /// </summary>
        public bool RemoveRootCollider = false;
    }

    public class FakeItem : InteractableObject
    {
        // I don't think this is needed, because the FakeItem will be destroyed every time it is reclaimed anyway
        //public delegate void InitializedHandler();
        //public event InitializedHandler OnInitialized;

        public delegate void SpawnedHandler();
        public event SpawnedHandler OnSpawned;

        public delegate void PlacedStateChangedHandler(bool isPlaced);
        public event PlacedStateChangedHandler OnPlacedStateChanged;

        public AddonFlags AddonFlags = new();
        public Dictionary<string, object> AddonData = [];

        private NavMeshObstacle _obstacle;

        private ObservedLootItem _lootItem;
        public ObservedLootItem LootItem
        {
            get
            {
                // this is necessary because there are some cases where the Item Id goes null, like picking up a backpack straight from the ground into your backpack slot
                if (_lootItem.Item == null || _lootItem.Item.Id == null)
                {
                    _lootItem = ItemHelper.GetLootItem(ItemId) as ObservedLootItem;
                }
                return _lootItem;
            }
            private set
            {
                _lootItem = value;
            }
        }
        public string ItemId { get; private set; }

        public List<CustomInteraction> Actions = new List<CustomInteraction>();
        public MoveableObject Moveable { get; private set; }
        private Vector3 _savedPosition;
        private Quaternion _savedRotation;

        private void Init(ObservedLootItem lootItem)
        {
            LootItem = lootItem;
            ItemId = lootItem.ItemId;
            AddNavMeshObstacle();
            Moveable = gameObject.AddComponent<MoveableObject>();
            LITSession.Instance.AddFakeItem(this); 
            if (LootItem.Item.IsContainer)
            {
                Actions.Add(GetSearchContainerAction());
            }
            Actions.Add(GetEnterMoveModeAction());

            LeaveItThereStaticEvents.InvokeOnFakeItemInitialized(this);

            // do this after event invokation to make sure IsPhysicalRegardlessOfSize flag is set correctly
            SetPlayerAndBotCollisionEnabled(Settings.PlacedItemsHaveCollision.Value);

            Actions.Add(GetReclaimAction());
            
            if (AddonFlags.RemoveRootCollider)
            {
                GetComponents<BoxCollider>().ExecuteForEach(Destroy);
            }
        }

        internal static FakeItem CreateNewFakeItem(ObservedLootItem lootItem, Dictionary<string, object> addonData = null)
        {
            GameObject obj = GameObject.Instantiate(lootItem.gameObject);
            ItemHelper.SetItemColor(Settings.PlacedItemTint.Value, obj.gameObject);
            obj.transform.position = lootItem.gameObject.transform.position;
            obj.transform.rotation = lootItem.gameObject.transform.rotation;
            obj.transform.localScale = obj.transform.localScale * 0.99f;

            ObservedLootItem componentWhoLivedComeToDie = obj.GetComponent<ObservedLootItem>();
            if (componentWhoLivedComeToDie != null)
            {
                Destroy(componentWhoLivedComeToDie); //hehe
            }

            FakeItem fakeItem = obj.AddComponent<FakeItem>();
            if (addonData != null)
            {
                fakeItem.AddonData = addonData;
            }
            fakeItem.Init(lootItem);
            return fakeItem;
        }

        private void AddNavMeshObstacle()
        {
            _obstacle = gameObject.GetOrAddComponent<NavMeshObstacle>();

            BoxCollider collider = gameObject.GetComponent<BoxCollider>();
            _obstacle.shape = NavMeshObstacleShape.Box;
            _obstacle.center = collider.center;
            _obstacle.size = collider.size;
            _obstacle.carving = true;
            _obstacle.carveOnlyStationary = false;
        }

        public void SetPlayerAndBotCollisionEnabled(bool enabled)
        {
            // if we are disabling the collision, always do it regardless of settings
            if (enabled)
            {
                bool itemIsTooSmall = LootItem.Item.Width * LootItem.Item.Height < Settings.MinimumSizeItemToGetCollision.Value;
                if (!AddonFlags.IsPhysicalRegardlessOfSize && itemIsTooSmall) return;
            }

            _obstacle.enabled = enabled;

            List<GameObject> descendants = Utils.GetAllDescendants(gameObject);
            foreach (GameObject descendant in descendants)
            {
                if (descendant.GetComponent<Collider>() == null) continue;
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

        public CustomInteraction GetSearchContainerAction()
        {
            GetActionsClass.Class1612 @class = new GetActionsClass.Class1612();
            @class.rootItem = LootItem.Item;
            @class.owner = Singleton<GameWorld>.Instance.MainPlayer.gameObject.GetComponent<GamePlayerOwner>();
            @class.lootItemLastOwner = LootItem.LastOwner;
            @class.lootItemOwner = LootItem.ItemOwner;
            @class.controller = @class.owner.Player.InventoryController;

            return new CustomInteraction("Search", false, @class.method_3);
        }

        public CustomInteraction GetReclaimAction()
        {
            // itemId is captured and passed into lambda
            string itemId = ItemId;

            return new CustomInteraction(
                () =>
                {
                    FakeItem fakeItem = LITSession.Instance.GetFakeItemOrNull(itemId);

                    if (fakeItem.AddonFlags.ReclaimInteractionDisabled)
                    {
                        return "Reclaim: Disabled";
                    }
                    else
                    {
                        return "Reclaim";
                    }
                },
                () =>
                {
                    FakeItem fakeItem = LITSession.Instance.GetFakeItemOrNull(itemId);
                    return fakeItem.AddonFlags.ReclaimInteractionDisabled;
                },
                () =>
                {
                    FakeItem fakeItem = LITSession.Instance.GetFakeItemOrNull(itemId);
                    FikaInterface.SendPlacedStateChangedPacket(fakeItem, false);
                    fakeItem.Reclaim();
                    fakeItem.ReclaimPlayerFeedback();
                }
            );
        }

        public void Reclaim()
        {
            LootItem.gameObject.transform.position = gameObject.transform.position;
            LootItem.gameObject.transform.rotation = gameObject.transform.rotation;
            LITSession.Instance.RefundPoints(ItemHelper.GetItemCost(LootItem.Item));
            LITSession.Instance.RemoveFakeItem(this);

            LeaveItThereStaticEvents.InvokeOnItemPlacedStateChanged(this, false);
            OnPlacedStateChanged?.Invoke(false);

            GameObject.Destroy(this.gameObject);
        }

        public void ReclaimPlayerFeedback()
        {
            var session = LITSession.Instance;
            if (Settings.CostSystemEnabled.Value)
            {
                InteractionHelper.NotificationLong($"Points rufunded: {ItemHelper.GetItemCost(LootItem.Item)}, {Settings.GetAllottedPoints() - session.PointsSpent} out of {Settings.GetAllottedPoints()} points remaining");
            }
            Singleton<GUISounds>.Instance.PlayUISound(EUISoundType.MenuWeaponDisassemble);
        }

        public static CustomInteraction GetPlaceItemAction(LootItem lootItem)
        {
            string itemId = lootItem.ItemId;

            return new CustomInteraction(
            "Place Item",
            !LITSession.Instance.PlacementIsAllowed(lootItem.Item),
            () =>
            {
                var lootItem = ItemHelper.GetLootItem(itemId) as ObservedLootItem;
                FakeItem fakeItem = CreateNewFakeItem(lootItem);
                fakeItem.PlaceAtLootItem();
                fakeItem.PlacedPlayerFeedback();
                FikaInterface.SendPlacedStateChangedPacket(fakeItem, true);
            });
        }

        public void PlaceAtLootItem()
        {
            LITSession.Instance.SpendPoints(ItemHelper.GetItemCost(LootItem.Item));
            SetLocation(LootItem.gameObject.transform.position, LootItem.gameObject.transform.rotation);
            LootItem.gameObject.transform.position = new Vector3(0, -99999, 0);

            LeaveItThereStaticEvents.InvokeOnItemPlacedStateChanged(this, true);
            OnPlacedStateChanged?.Invoke(true);
        }

        public void PlaceAtPosition(Vector3 position, Quaternion rotation)
        {
            PlaceAtLootItem();
            SetLocation(position, rotation);
        }

        public void PlacedPlayerFeedback()
        {
            var session = LITSession.Instance;
            if (Settings.CostSystemEnabled.Value)
            {
                InteractionHelper.NotificationLong($"Placement cost: {ItemHelper.GetItemCost(LootItem.Item)}, {Settings.GetAllottedPoints() - session.PointsSpent} out of {Settings.GetAllottedPoints()} points remaining");
            }
            Singleton<GUISounds>.Instance.PlayUISound(EUISoundType.MenuWeaponAssemble);
        }

        public CustomInteraction GetEnterMoveModeAction()
        {
            // itemId is captured and passed into lambdas to avoid a closure of 'this'
            string itemId = ItemId;
            return new CustomInteraction(
                () =>
                {
                    FakeItem fakeItem = LITSession.Instance.GetFakeItemOrNull(itemId);
                    if (fakeItem.MoveModeDisallowed(out string reason))
                    {
                        return $"Move: {reason}";
                    }
                    else
                    {
                        return "Move";
                    }
                },
                LITSession.Instance.GetFakeItemOrNull(itemId).MoveModeDisallowed,
                () =>
                {
                    FakeItem fakeItem = LITSession.Instance.GetFakeItemOrNull(itemId);
                    var mover = ObjectMover.Instance;
                    mover.Enable(fakeItem.gameObject.GetComponent<MoveableObject>(), fakeItem.OnMoveModeDisabled, fakeItem.OnMoveModeEnabledUpdate);
                    fakeItem._savedPosition = fakeItem.gameObject.transform.position;
                    fakeItem._savedRotation = fakeItem.gameObject.transform.rotation;
                    fakeItem.SetPlayerAndBotCollisionEnabled(false);
                },
                LootItem.Name.Localized()
            );
        }

        private void OnMoveModeEnabledUpdate()
        {
            var mover = ObjectMover.Instance;

            if (MoveModeDisallowed())
            {
                mover.Disable(true);
                ErrorPlayerFeedback("Not enough space in inventory to move item!");
                FikaInterface.SendPlacedStateChangedPacket(this, true, Settings.ImmersivePhysics.Value);
                return;
            }
            if (Settings.MoveModeCancelsSprinting.Value && LITSession.Instance.Player.Physical.Sprinting)
            {
                mover.Disable(true);
                ErrorPlayerFeedback("'MOVE' mode cancelled.");
                FikaInterface.SendPlacedStateChangedPacket(this, true, Settings.ImmersivePhysics.Value);
                return;
            }
        }

        private void OnMoveModeDisabled(bool saved)
        {
            InteractionHelper.RefreshPrompt();

            if (saved)
            {
                PlaceAtPosition(gameObject.transform.position, gameObject.transform.rotation);
                FikaInterface.SendPlacedStateChangedPacket(this, true, Settings.ImmersivePhysics.Value);
            }
            else
            {
                ResetPosition();
            }

            SetPlayerAndBotCollisionEnabled(Settings.PlacedItemsHaveCollision.Value);
        }

        public bool MoveModeDisallowed(out string reason)
        {
            if (AddonFlags.MoveModeDisabled)
            {
                reason = "Disabled";
                return true;
            }
            if (Settings.MoveModeRequiresInventorySpace.Value && !ItemHelper.ItemCanBePickedUp(LootItem.Item))
            {
                reason = "No Space";
                return true;
            }

            reason = "";
            return false;
        }

        public bool MoveModeDisallowed()
        {
            return MoveModeDisallowed(out string _);
        }

        private void ResetPosition()
        {
            gameObject.transform.position = _savedPosition;
            gameObject.transform.rotation = _savedRotation;
        }

        private void SetLocation(Vector3 position, Quaternion rotation)
        {
            gameObject.transform.position = position;
            gameObject.transform.rotation = rotation;
        }

        public static void ErrorPlayerFeedback(string message)
        {
            InteractionHelper.NotificationLongWarning(message);
            Singleton<GUISounds>.Instance.PlayUISound(EUISoundType.ErrorMessage);
        }

        public void InvokeOnSpawnedEvent()
        {
            OnSpawned?.Invoke();
        }

        public T GetAddonDataOrNull<T>(string key) where T : class
        {
            if (!AddonData.ContainsKey(key)) return null;
            if (AddonData[key] is not T)
            {
                object data = AddonData[key];
                T typedData = JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(data));
                AddonData[key] = typedData;
            }
            return (T)AddonData[key];
        }

        public void PutAddonData<T>(string key, T data) where T : class
        {
            AddonData[key] = data;
        }
    }
}
