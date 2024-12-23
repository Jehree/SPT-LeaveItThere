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

        public static RemoteInteractable GetOrCreateRemoteInteractable(Vector3 position, LootItem lootItem, ItemRemotePair pair = null)
        {
            if (pair == null)
            {
                return CreateNewRemoteInteractable(position, lootItem.gameObject.transform.rotation, lootItem as ObservedLootItem);
            }
            else
            {
                pair.RemoteInteractable.gameObject.transform.position = position;
                pair.RemoteInteractable.gameObject.transform.rotation = lootItem.gameObject.transform.rotation;
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
            remoteInteractable.Actions.Add(moveable.GetEnterMoveModeAction(OnMoveModeExited));
            remoteInteractable.Actions.Add(remoteInteractable.GetDemolishItemAction());

            remoteAccessObj.transform.position = position;
            remoteAccessObj.transform.rotation = rotation;
            remoteAccessObj.transform.localScale = remoteAccessObj.transform.localScale * 0.99f;

            return remoteInteractable;
        }

        private static void OnMoveModeExited(GameObject gameObject)
        {
            InteractionHelper.RefreshPrompt(true);
            var session = ModSession.GetSession();
            var remoteInteractable = gameObject.GetComponent<RemoteInteractable>();
            var pair = session.GetPairOrNull(remoteInteractable);
            session.AddOrUpdatePair(pair.LootItem, remoteInteractable, remoteInteractable.gameObject.transform.position, true);
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
                () =>
                {
                    DemolishItem();
                }
            );
        }

        public  void DemolishItem()
        {
            var session = ModSession.GetSession();
            var pair = session.GetPairOrNull(this);

            gameObject.transform.position = new Vector3(0, -9999, 0);
            pair.LootItem.gameObject.transform.position = pair.PlacementPosition;
            pair.LootItem.gameObject.transform.rotation = gameObject.transform.rotation;
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
    }
}
