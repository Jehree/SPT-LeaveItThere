using Comfort.Common;
using EFT;
using EFT.Interactive;
using EFT.InventoryLogic;
using EFT.UI;
using InteractableInteractionsAPI.Common;
using LeaveItThere.Common;
using LeaveItThere.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LeaveItThere.Components
{
    internal class RemoteInteractable : InteractableObject
    {
        public List<CustomInteraction> Actions = new List<CustomInteraction>();

        public static RemoteInteractable GetOrCreateRemoteInteractable(Vector3 position, Quaternion rotation, LootItem lootItem, ItemRemotePair pair = null)
        {
            if (pair == null)
            {
                return CreateNewRemoteInteractable(position, rotation, lootItem as ObservedLootItem);
            }
            else
            {
                pair.RemoteInteractable.gameObject.transform.position = position;
                pair.RemoteInteractable.gameObject.transform.rotation = rotation;
                ItemHelper.SetItemColor(Settings.PlacedItemTint.Value, pair.RemoteInteractable.gameObject);
                return pair.RemoteInteractable;
            }
        }

        private static RemoteInteractable CreateNewRemoteInteractable(Vector3 position, Quaternion rotation, ObservedLootItem lootItem)
        {
            GameObject remoteAccessObj = GameObject.Instantiate(lootItem.gameObject);

            ObservedLootItem componentWhoLivedComeToDie = remoteAccessObj.GetComponent<ObservedLootItem>();
            if (componentWhoLivedComeToDie != null)
            {
                GameObject.Destroy(componentWhoLivedComeToDie); //hehe
            }

            RemoteInteractable remoteInteractable = remoteAccessObj.AddComponent<RemoteInteractable>();
            ItemHelper.SetItemColor(Settings.PlacedItemTint.Value, remoteAccessObj.gameObject);
            if (lootItem.Item.IsContainer)
            {
                remoteInteractable.Actions.Add(RemoteInteractable.GetRemoteOpenItemAction(lootItem));

            }
            MoveableObject moveable = remoteAccessObj.AddComponent<MoveableObject>();
            CustomInteraction enterMoveModeAction = remoteInteractable.GetEnterMoveModeAction(moveable);
            remoteInteractable.Actions.Add(enterMoveModeAction);
            remoteInteractable.Actions.Add(remoteInteractable.GetDemolishItemAction());

            remoteAccessObj.transform.position = position;
            remoteAccessObj.transform.rotation = rotation;
            remoteAccessObj.transform.localScale = remoteAccessObj.transform.localScale * 0.99f;

            return remoteInteractable;
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
            return new CustomInteraction(
                "Reclaim",
                false,
                DemolishItem
            );
        }

        public  void DemolishItem()
        {
            var session = ModSession.GetSession();
            var pair = session.GetPairOrNull(this);

            gameObject.transform.position = new Vector3(0, -9999, 0);
            pair.LootItem.gameObject.transform.position = pair.PlacementPosition;
            pair.LootItem.gameObject.transform.rotation = pair.PlacementRotation;
            pair.Placed = false;
            session.PointsSpent -= ItemHelper.GetItemCost(pair.LootItem.Item);

            DemolishPlayerFeedback(pair.LootItem.Item);
        }

        public static void DemolishPlayerFeedback(Item item)
        {
            var session = ModSession.GetSession();
            if (Settings.CostSystemEnabled.Value)
            {
                InteractionHelper.NotificationLong($"Points rufunded: {ItemHelper.GetItemCost(item)}, {Settings.GetAllottedPoints() - session.PointsSpent} out of {Settings.GetAllottedPoints()} points remaining");
            }
            Singleton<GUISounds>.Instance.PlayUISound(EUISoundType.MenuWeaponDisassemble);
        }

        public CustomInteraction GetEnterMoveModeAction(MoveableObject moveable)
        {
            return new CustomInteraction(
                () =>
                {
                    if (MoveModeDisallowed())
                    {
                        return "Move: No Space";
                    }
                    else
                    {
                        return "Move";
                    }
                },
                MoveModeDisallowed,
                () =>
                {
                    if (MoveModeDisallowed())
                    {
                        NotEnoughSpaceForMoveModePlayerFeedback();
                        return;
                    }
                    moveable.EnterMoveMode(OnMoveModeExited, OnMoveModeActiveUpdate);
                    ModSession.GetSession().Player.EnableSprint(false);
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

        private void OnMoveModeExited(MoveableObject moveable)
        {
            InteractionHelper.RefreshPrompt(true);
            var session = ModSession.GetSession();
            var pair = session.GetPairOrNull(this);
            session.AddOrUpdatePair(pair.LootItem, this, gameObject.transform.position, gameObject.transform.rotation, true);
            session.Player.EnableSprint(true);
        }

        private void OnMoveModeActiveUpdate(MoveableObject moveable)
        {
            if (MoveModeDisallowed())
            {
                moveable.ExitMoveMode();
                NotEnoughSpaceForMoveModePlayerFeedback();
                return;
            }
            var session = ModSession.GetSession();
            if (Settings.MoveModeCancelsSprinting.Value && session.Player.Physical.Sprinting)
            {
                moveable.ExitMoveMode();
                InteractionHelper.NotificationLong("'MOVE' mode cancelled.");
            }
        }

        public static void NotEnoughSpaceForMoveModePlayerFeedback()
        {
            InteractionHelper.NotificationLongWarning("Not enough space in inventory to move item!");
            Singleton<GUISounds>.Instance.PlayUISound(EUISoundType.ErrorMessage);
        }
    }
}
