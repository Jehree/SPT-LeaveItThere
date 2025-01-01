using Comfort.Common;
using EFT;
using EFT.Interactive;
using EFT.UI;
using InteractableInteractionsAPI.Common;
using LeaveItThere.Fika;
using LeaveItThere.Helpers;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace LeaveItThere.Components
{
    internal class FakeItem : InteractableObject
    {
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
        public Vector3 WorldPosition
        {
            get
            {
                if (Placed)
                {
                    return gameObject.transform.position;
                }
                else
                {
                    return LootItem.gameObject.transform.position;
                }
            }
        }
        public Quaternion WorldRotation
        {
            get
            {
                if (Placed)
                {
                    return gameObject.transform.rotation;
                }
                else
                {
                    return LootItem.gameObject.transform.rotation;
                }
            }
        }
        public string ItemId { get; private set; }
        public bool Placed { get; set; }

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
            ModSession.Instance.AddFakeItem(this);

            if (LootItem.Item.IsContainer)
            {
                Actions.Add(GetSearchContainerAction());
            }
            Actions.Add(GetEnterMoveModeAction());
            Actions.Add(GetReclaimAction());

            SetPlayerAndBotCollisionEnabled(Settings.PlacedItemsHaveCollision.Value);
        }

        public static FakeItem CreateNewRemoteInteractable(ObservedLootItem lootItem)
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
                    FakeItem fakeItem = ModSession.Instance.GetFakeItemOrNull(itemId);
                    fakeItem.Reclaim();
                    fakeItem.ReclaimPlayerFeedback();
                    FikaInterface.SendPlacedStateChangedPacket(fakeItem);
                }
            );
        }

        public void Reclaim()
        {
            if (!Placed) return;

            LootItem.gameObject.transform.position = gameObject.transform.position;
            LootItem.gameObject.transform.rotation = gameObject.transform.rotation;
            gameObject.transform.position = new Vector3(0, -9999, 0);
            Placed = false;
            ModSession.Instance.RefundPoints(ItemHelper.GetItemCost(LootItem.Item));
        }

        public void ReclaimPlayerFeedback()
        {
            var session = ModSession.Instance;
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
            !ModSession.Instance.PlacementIsAllowed(lootItem.Item),
            () =>
            {
                FakeItem fakeItem;
                var lootItem = ItemHelper.GetLootItem(itemId) as ObservedLootItem;
                if (ModSession.Instance.TryGetFakeItem(itemId, out FakeItem fakeItemFetch))
                {
                    fakeItem = fakeItemFetch;
                }
                else
                {
                    fakeItem = CreateNewRemoteInteractable(lootItem);
                }

                fakeItem.PlaceAtLootItem();
                fakeItem.PlacedPlayerFeedback();
                FikaInterface.SendPlacedStateChangedPacket(fakeItem);
            });
        }

        public void PlaceAtLootItem()
        {
            ModSession.Instance.SpendPoints(ItemHelper.GetItemCost(LootItem.Item));
            SetLocation(LootItem.gameObject.transform.position, LootItem.gameObject.transform.rotation);
            LootItem.gameObject.transform.position = new Vector3(0, -99999, 0);
            Placed = true;
        }

        public void PlaceAtLocation(Vector3 position, Quaternion rotation)
        {
            PlaceAtLootItem();
            SetLocation(position, rotation);
        }

        public void PlacedPlayerFeedback()
        {
            var session = ModSession.Instance;
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
                    FakeItem fakeItem = ModSession.Instance.GetFakeItemOrNull(itemId);
                    if (fakeItem.MoveModeDisallowed())
                    {
                        return "Move: No Space";
                    }
                    else
                    {
                        return "Move";
                    }
                },
                ModSession.Instance.GetFakeItemOrNull(itemId).MoveModeDisallowed,
                () =>
                {
                    FakeItem fakeItem = ModSession.Instance.GetFakeItemOrNull(itemId);
                    var mover = ObjectMover.GetMover();
                    mover.Enable(fakeItem.gameObject.GetComponent<MoveableObject>(), fakeItem.OnMoveModeDisabled, fakeItem.OnMoveModeEnabledUpdate);
                    fakeItem._savedPosition = fakeItem.gameObject.transform.position;
                    fakeItem._savedRotation = fakeItem.gameObject.transform.rotation;
                    fakeItem.SetPlayerAndBotCollisionEnabled(false);
                }
            );
        }

        private void OnMoveModeEnabledUpdate()
        {
            var mover = ObjectMover.GetMover();

            if (MoveModeDisallowed())
            {
                mover.Disable(true);
                ErrorPlayerFeedback("Not enough space in inventory to move item!");
                FikaInterface.SendPlacedStateChangedPacket(this, Settings.ImmersivePhysics.Value);
                return;
            }
            if (Settings.MoveModeCancelsSprinting.Value && ModSession.Instance.Player.Physical.Sprinting)
            {
                mover.Disable(true);
                ErrorPlayerFeedback("'MOVE' mode cancelled.");
                FikaInterface.SendPlacedStateChangedPacket(this, Settings.ImmersivePhysics.Value);
                return;
            }
        }

        private void OnMoveModeDisabled(bool saved)
        {
            InteractionHelper.RefreshPrompt(true);

            if (saved)
            {
                PlaceAtLocation(gameObject.transform.position, gameObject.transform.rotation);
                FikaInterface.SendPlacedStateChangedPacket(this, Settings.ImmersivePhysics.Value);
            }
            else
            {
                ResetPosition();
            }

            SetPlayerAndBotCollisionEnabled(Settings.PlacedItemsHaveCollision.Value);
        }

        private bool MoveModeDisallowed()
        {
            if (!Settings.MoveModeRequiresInventorySpace.Value) return false;
            if (!ItemHelper.ItemCanBePickedUp(LootItem.Item)) return true;
            return false;
        }

        private void ResetPosition()
        {
            if (Placed)
            {
                gameObject.transform.position = _savedPosition;
                gameObject.transform.rotation = _savedRotation;
            }
            else
            {
                gameObject.transform.position = new Vector3(0, -99999, 0);
                gameObject.transform.rotation = LootItem.gameObject.transform.rotation;
            }
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
    }
}
