using Comfort.Common;
using EFT;
using EFT.Interactive;
using EFT.InventoryLogic;
using EFT.UI;
using InteractableInteractionsAPI.Common;
using LeaveItThere.Common;
using LeaveItThere.Fika;
using LeaveItThere.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LeaveItThere.Components
{
    internal class RemoteInteractable : InteractableObject
    {
        public List<CustomInteraction> Actions = new List<CustomInteraction>();

        public static RemoteInteractable CreateNewRemoteInteractable(Vector3 position, Quaternion rotation, ObservedLootItem lootItem)
        {
            GameObject remoteAccessObj = GameObject.Instantiate(lootItem.gameObject);

            ObservedLootItem componentWhoLivedComeToDie = remoteAccessObj.GetComponent<ObservedLootItem>();
            if (componentWhoLivedComeToDie != null)
            {
                GameObject.Destroy(componentWhoLivedComeToDie); //hehe
            }

            RemoteInteractable remoteInteractable = remoteAccessObj.AddComponent<RemoteInteractable>();
            ItemHelper.SetItemColor(Settings.PlacedItemTint.Value, remoteAccessObj.gameObject);

            remoteAccessObj.transform.position = position;
            remoteAccessObj.transform.rotation = rotation;
            remoteAccessObj.transform.localScale = remoteAccessObj.transform.localScale * 0.99f;

            return remoteInteractable;
        }

        // interactions can't be added until the pair exists, so the ModSession class takes care of that in AddPair()
        public void InitInteractions(ObservedLootItem lootItem)
        {
            if (lootItem.Item.IsContainer)
            {
                Actions.Add(RemoteInteractable.GetRemoteOpenItemAction(lootItem));
            }
            Actions.Add(GetEnterMoveModeAction());
            Actions.Add(GetDemolishItemAction());

        }

        public static CustomInteraction GetRemoteOpenItemAction(LootItem lootItem)
        {
            GetActionsClass.Class1612 @class = new GetActionsClass.Class1612();
            @class.rootItem = lootItem.Item;
            @class.owner = Singleton<GameWorld>.Instance.MainPlayer.gameObject.GetComponent<GamePlayerOwner>();
            @class.lootItemLastOwner = lootItem.LastOwner;
            @class.lootItemOwner = lootItem.ItemOwner;
            @class.controller = @class.owner.Player.InventoryController;


            return new CustomInteraction("Search", false, @class.method_3);
        }

        public CustomInteraction GetDemolishItemAction()
        {
            // itemId is captured and passed into lambda
            string itemId = ModSession.GetSession().GetPairOrNull(this).LootItem.ItemId;

            return new CustomInteraction(
                "Reclaim",
                false,
                () =>
                {
                    var pair = ModSession.GetSession().GetPairOrNull(itemId);
                    pair.RemoteInteractable.DemolishItem();
                    pair.RemoteInteractable.DemolishPlayerFeedback();
                    FikaInterface.SendPlacedStateChangedPacket(pair);
                }
            );
        }

        public void DemolishItem()
        {
            var session = ModSession.GetSession();
            var pair = session.GetPairOrNull(this);
            if (!pair.Placed) return;

            gameObject.transform.position = new Vector3(0, -9999, 0);
            pair.LootItem.gameObject.transform.position = pair.PlacementPosition;
            pair.LootItem.gameObject.transform.rotation = pair.PlacementRotation;
            pair.Placed = false;
            session.PointsSpent -= ItemHelper.GetItemCost(pair.LootItem.Item);
        }

        public void DemolishPlayerFeedback()
        {
            var session = ModSession.GetSession();
            var pair = session.GetPairOrNull(this);
            Item item = pair.LootItem.Item;
            if (Settings.CostSystemEnabled.Value)
            {
                InteractionHelper.NotificationLong($"Points rufunded: {ItemHelper.GetItemCost(item)}, {Settings.GetAllottedPoints() - session.PointsSpent} out of {Settings.GetAllottedPoints()} points remaining");
            }
            Singleton<GUISounds>.Instance.PlayUISound(EUISoundType.MenuWeaponDisassemble);
        }

        public CustomInteraction GetEnterMoveModeAction()
        {
            // itemId is captured and passed into lambdas to avoid a closure of 'this'
            string itemId = ModSession.GetSession().GetPairOrNull(this).LootItem.ItemId;
            return new CustomInteraction(
                () =>
                {
                    RemoteInteractable interactable = ModSession.GetSession().GetPairOrNull(itemId).RemoteInteractable;
                    if (interactable.MoveModeDisallowed())
                    {
                        return "Move: No Space";
                    }
                    else
                    {
                        return "Move";
                    }
                },
                ModSession.GetSession().GetPairOrNull(itemId).RemoteInteractable.MoveModeDisallowed,
                () =>
                {
                    RemoteInteractable interactable = ModSession.GetSession().GetPairOrNull(itemId).RemoteInteractable;
                    var mover = ObjectMover.GetMover();
                    if (interactable.MoveModeDisallowed())
                    {
                        NotEnoughSpaceForMoveModePlayerFeedback();
                        return;
                    }
                    mover.Enable(interactable.gameObject, interactable.OnMoveModeDisabled, interactable.OnMoveModeEnabledUpdate);
                }
            );
        }

        private bool MoveModeDisallowed()
        {
            if (!Settings.MoveModeRequiresInventorySpace.Value) return false;
            var session = ModSession.GetSession();
            LootItem lootItem = session.GetPairOrNull(this).LootItem;
            if (!ItemHelper.ItemCanBePickedUp(lootItem.Item)) return true;
            return false;
        }

        private void OnMoveModeDisabled(GameObject target, bool save)
        {
            InteractionHelper.RefreshPrompt(true);
            var session = ModSession.GetSession();
            var pair = session.GetPairOrNull(this);

            if (save)
            {
                ItemPlacer.PlaceItem(pair.LootItem, gameObject.transform.position, gameObject.transform.rotation);
                FikaInterface.SendPlacedStateChangedPacket(pair);
            }
            else
            {
                if (pair.Placed)
                {
                    gameObject.transform.position = pair.PlacementPosition;
                    gameObject.transform.rotation = pair.PlacementRotation;
                }
                else
                {
                    gameObject.transform.position = new Vector3(0, -99999, 0);
                    gameObject.transform.rotation = pair.PlacementRotation;
                }
            }
        }

        private void OnMoveModeEnabledUpdate(GameObject target)
        {
            var session = ModSession.GetSession();
            var mover = ObjectMover.GetMover();

            if (MoveModeDisallowed())
            {
                mover.Disable(true);
                NotEnoughSpaceForMoveModePlayerFeedback();
                FikaInterface.SendPlacedStateChangedPacket(session.GetPairOrNull(this));
                return;
            }
            if (Settings.MoveModeCancelsSprinting.Value && session.Player.Physical.Sprinting)
            {
                mover.Disable(true);
                FikaInterface.SendPlacedStateChangedPacket(session.GetPairOrNull(this));
                InteractionHelper.NotificationLong("'MOVE' mode cancelled.");
                return;
            }
        }

        public static void NotEnoughSpaceForMoveModePlayerFeedback()
        {
            InteractionHelper.NotificationLongWarning("Not enough space in inventory to move item!");
            Singleton<GUISounds>.Instance.PlayUISound(EUISoundType.ErrorMessage);
        }
    }
}
