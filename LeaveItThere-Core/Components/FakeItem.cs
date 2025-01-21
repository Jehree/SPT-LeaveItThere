﻿using Comfort.Common;
using EFT;
using EFT.Interactive;
using EFT.UI;
using LeaveItThere.Common;
using LeaveItThere.Fika;
using LeaveItThere.Helpers;
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace LeaveItThere.Components
{
    public class AddonFlags
    {
        public bool MoveModeDisabled = false;
        public bool ImmersivePhysicsForceDisabled = false;
    }

    public class FakeItem : InteractableObject
    {
        public delegate void InitializedHandler();
        public event InitializedHandler OnInitialized;

        public delegate void SpawnedHandler();
        public event SpawnedHandler OnSpawned;

        public delegate void PlacedStateChangedHandler(bool isPlaced);
        public event PlacedStateChangedHandler OnPlacedStateChanged;

        public AddonFlags AddonFlags = new();
        public Dictionary<string, object> AddonData = [];

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
            SetPlayerAndBotCollisionEnabled(Settings.PlacedItemsHaveCollision.Value);

            LeaveItThereStaticEvents.InvokeOnFakeItemInitialized(this);
            OnInitialized?.Invoke();
            // add this after event invocations to make sure the Reclaim action is always last
            Actions.Add(GetReclaimAction());
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
                GameObject.Destroy(componentWhoLivedComeToDie); //hehe
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
            NavMeshObstacle obstacle = gameObject.AddComponent<NavMeshObstacle>();
            if (obstacle == null)
            {
                throw new System.Exception("Tried to add a NavMeshObstacle to a FakeItem that already had one!");
            }

            BoxCollider collider = gameObject.GetComponent<BoxCollider>();
            obstacle.shape = NavMeshObstacleShape.Box;
            obstacle.center = collider.center;
            obstacle.size = collider.size;
            obstacle.carving = true;
            obstacle.carveOnlyStationary = false;
        }

        public void SetPlayerAndBotCollisionEnabled(bool enabled)
        {
            gameObject.GetComponent<NavMeshObstacle>().enabled = enabled;
            List<GameObject> descendants = Utils.GetAllDescendants(gameObject);
            foreach (GameObject descendant in descendants)
            {
                if (
                    descendant.GetComponent<BoxCollider>() == null &&
                    descendant.GetComponent<MeshCollider>() == null
                ) continue;

                if (LootItem.Item.Width * LootItem.Item.Height < Settings.MinimumSizeItemToGetCollision.Value) continue;
                if (enabled)
                {
                    descendant.layer = LayerMask.NameToLayer("Default");
                }
                else
                {
                    descendant.layer = LayerMask.NameToLayer("Loot");
                }
            }
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
                "Reclaim",
                false,
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
                }
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
