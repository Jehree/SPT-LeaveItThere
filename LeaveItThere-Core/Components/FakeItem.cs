using Comfort.Common;
using EFT;
using EFT.Interactive;
using EFT.UI;
using LeaveItThere.Addon;
using LeaveItThere.Common;
using LeaveItThere.Fika;
using LeaveItThere.Helpers;
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace LeaveItThere.Components
{
    public class FakeItem : InteractableObject
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

        public delegate void SpawnedHandler();
        /// <summary>
        /// Invoked if this FakeItem was just spawned by LITSession at start of raid
        /// </summary>
        public event SpawnedHandler OnSpawned;
        internal void InvokeOnFakeItemSpawned() => OnSpawned?.Invoke();

        public delegate void PlacedStateChangedHandler(bool isPlaced);
        /// <summary>
        /// Invoked any time the placed state of the FakeItem is changed. Unike OnFakeItemInitialized, it will be invoked when item is moved.
        /// </summary>
        public event PlacedStateChangedHandler OnPlacedStateChanged;

        public AddonFlags Flags { get; private set; } = new();
        public Dictionary<string, object> AddonData { get; private set; } = [];

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
        public string TemplateId { get; private set; }

        public List<CustomInteraction> Interactions = [];
        public MoveableObject Moveable { get; private set; }
        private Vector3 _savedPosition;
        private Quaternion _savedRotation;

        private void Init(ObservedLootItem lootItem)
        {
            LootItem = lootItem;
            ItemId = lootItem.ItemId;
            TemplateId = lootItem.TemplateId;
            AddNavMeshObstacle();
            Moveable = gameObject.AddComponent<MoveableObject>();

            LITSession.Instance.AddFakeItem(this);
            if (LootItem.Item.IsContainer)
            {
                Interactions.Add(new SearchInteraction(this));
            }
            Interactions.Add(new EnterMoveModeInteraction(this));

            LITStaticEvents.InvokeOnFakeItemInitialized(this);

            // do this after event invokation to make sure IsPhysicalRegardlessOfSize flag is set correctly
            SetPlayerAndBotCollisionEnabled(Settings.PlacedItemsHaveCollision.Value);

            Interactions.Add(new ReclaimInteraction(this));

            if (Flags.RemoveRootCollider || gameObject.name.Contains("LITRemoveRootCollider"))
            {
                GetComponents<BoxCollider>().ExecuteForEach(col => col.enabled = false);
            }
        }

        internal static FakeItem CreateNewFakeItem(ObservedLootItem lootItem, Dictionary<string, object> addonData = null)
        {
            GameObject obj = Instantiate(lootItem.gameObject);
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
                if (!Flags.IsPhysicalRegardlessOfSize && itemIsTooSmall) return;
            }

            _obstacle.enabled = enabled;

            List<GameObject> descendants = LITUtils.GetAllDescendants(gameObject);
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

        public void Reclaim()
        {
            LootItem.gameObject.transform.position = gameObject.transform.position;
            LootItem.gameObject.transform.rotation = gameObject.transform.rotation;
            LITSession.Instance.RefundPoints(ItemHelper.GetItemCost(LootItem.Item));
            LITSession.Instance.RemoveFakeItem(this);

            LITStaticEvents.InvokeOnItemPlacedStateChanged(this, false);
            OnPlacedStateChanged?.Invoke(false);

            GameObject.Destroy(this.gameObject);
        }

        public void ReclaimPlayerFeedback()
        {
            LITSession session = LITSession.Instance;
            if (Settings.CostSystemEnabled.Value)
            {
                InteractionHelper.NotificationLong($"Points rufunded: {ItemHelper.GetItemCost(LootItem.Item)}, {Settings.GetAllottedPoints() - session.PointsSpent} out of {Settings.GetAllottedPoints()} points remaining");
            }
            Singleton<GUISounds>.Instance.PlayUISound(EUISoundType.MenuWeaponDisassemble);
        }

        public void PlaceAtLootItem()
        {
            LootItem.StopPhysics();
            LITSession.Instance.SpendPoints(ItemHelper.GetItemCost(LootItem.Item));
            SetLocation(LootItem.gameObject.transform.position, LootItem.gameObject.transform.rotation);
            LootItem.gameObject.transform.position = new Vector3(0, -99999, 0);

            LITStaticEvents.InvokeOnItemPlacedStateChanged(this, true);
            OnPlacedStateChanged?.Invoke(true);
        }

        public void PlaceAtPosition(Vector3 position, Quaternion rotation)
        {
            PlaceAtLootItem();
            SetLocation(position, rotation);
        }

        public void PlacedPlayerFeedback()
        {
            LITSession session = LITSession.Instance;
            if (Settings.CostSystemEnabled.Value)
            {
                InteractionHelper.NotificationLong($"Placement cost: {ItemHelper.GetItemCost(LootItem.Item)}, {Settings.GetAllottedPoints() - session.PointsSpent} out of {Settings.GetAllottedPoints()} points remaining");
            }
            Singleton<GUISounds>.Instance.PlayUISound(EUISoundType.MenuWeaponAssemble);
        }

        private void OnMoveModeEnabledUpdate()
        {
            ObjectMover mover = ObjectMover.Instance;

            if (MoveModeDisallowed())
            {
                mover.Disable(true);
                ErrorPlayerFeedback("Not enough space in inventory to move item!");
                FikaBridge.SendPlacedStateChangedPacket(this, true, Settings.ImmersivePhysics.Value);
                return;
            }
            if (Settings.MoveModeCancelsSprinting.Value && LITSession.Instance.Player.Physical.Sprinting)
            {
                mover.Disable(true);
                ErrorPlayerFeedback("'MOVE' mode cancelled.");
                FikaBridge.SendPlacedStateChangedPacket(this, true, Settings.ImmersivePhysics.Value);
                return;
            }
        }

        private void OnMoveModeDisabled(bool saved)
        {
            InteractionHelper.RefreshPrompt();

            if (saved)
            {
                PlaceAtPosition(gameObject.transform.position, gameObject.transform.rotation);
                FikaBridge.SendPlacedStateChangedPacket(this, true, Settings.ImmersivePhysics.Value);
            }
            else
            {
                ResetPosition();
            }

            SetPlayerAndBotCollisionEnabled(Settings.PlacedItemsHaveCollision.Value);
        }

        public bool MoveModeDisallowed(out string reason)
        {
            if (Flags.MoveModeDisabled)
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

        /// <summary>
        /// Fetches Addon Data on this FakeItem by it's key, returns null if none is found. T must be a class.
        /// </summary>
        /// <typeparam name="T">Must be a class. After Addon Data is fetched once, it can usually be modified directly without needing to use PutAddonData again since classes are stored by reference.</typeparam>
        /// <param name="key">Key that Addon Data was saved under.</param>
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

        /// <summary>
        /// Saves Addon Data by a given key. Data will be saved to user/profiles/LeaveItThere-ItemData under the FakeItem that the data is added to.
        /// </summary>
        /// <typeparam name="T">Must be a class.</typeparam>
        /// <param name="key">Key used to fetch the data later.</param>
        /// <param name="data"></param>
        public void PutAddonData<T>(string key, T data) where T : class
        {
            AddonData[key] = data;
        }

        public class SearchInteraction : CustomInteraction
        {
            private GetActionsClass.Class1622 _searchClass;

            public SearchInteraction(FakeItem fakeItem) : base(fakeItem)
            {
                _searchClass = new();
                _searchClass.rootItem = FakeItem.LootItem.Item;
                _searchClass.owner = Singleton<GameWorld>.Instance.MainPlayer.gameObject.GetComponent<GamePlayerOwner>();
                _searchClass.lootItemLastOwner = FakeItem.LootItem.LastOwner;
                _searchClass.lootItemOwner = FakeItem.LootItem.ItemOwner;
                _searchClass.controller = _searchClass.owner.Player.InventoryController;
            }

            public override string Name => "Search";
            public override void OnInteract() => _searchClass.method_3();
        }

        public class PlaceItemInteraction(LootItem lootItem) : CustomInteraction
        {
            private LootItem _lootItem = lootItem;
            public override string Name => "Place Item";
            public override bool Enabled => LITSession.Instance.PlacementIsAllowed(_lootItem.Item);

            public override void OnInteract()
            {
                FakeItem fakeItem = CreateNewFakeItem(_lootItem as ObservedLootItem);
                fakeItem.PlaceAtLootItem();
                fakeItem.PlacedPlayerFeedback();
                FikaBridge.SendPlacedStateChangedPacket(fakeItem, true);
            }
        }

        public class EnterMoveModeInteraction(FakeItem fakeItem) : CustomInteraction(fakeItem)
        {
            public override string Name
            {
                get
                {
                    if (FakeItem.MoveModeDisallowed(out string reason))
                    {
                        return $"Move: {reason}";
                    }
                    else
                    {
                        return "Move";
                    }
                }
            }

            public override bool Enabled => !FakeItem.MoveModeDisallowed();

            public override void OnInteract()
            {
                ObjectMover mover = ObjectMover.Instance;
                mover.Enable(FakeItem.gameObject.GetComponent<MoveableObject>(), FakeItem.OnMoveModeDisabled, FakeItem.OnMoveModeEnabledUpdate);
                FakeItem._savedPosition = FakeItem.gameObject.transform.position;
                FakeItem._savedRotation = FakeItem.gameObject.transform.rotation;
                FakeItem.SetPlayerAndBotCollisionEnabled(false);
            }
        }

        public class ReclaimInteraction(FakeItem fakeItem) : CustomInteraction(fakeItem)
        {
            public override string Name => FakeItem.Flags.ReclaimInteractionDisabled
                                                                    ? "Reclaim: Disabled"
                                                                    : "Reclaim";
            public override string TargetName => FakeItem.LootItem.Name.Localized();
            public override bool Enabled => !FakeItem.Flags.ReclaimInteractionDisabled;
            public override bool AutoPromptRefresh => true;

            public override void OnInteract()
            {
                FikaBridge.SendPlacedStateChangedPacket(FakeItem, false);
                FakeItem.Reclaim();
                FakeItem.ReclaimPlayerFeedback();
            }
        }
    }
}