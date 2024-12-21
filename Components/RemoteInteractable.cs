using Comfort.Common;
using EFT;
using EFT.Interactive;
using InteractableInteractionsAPI.Common;
using LeaveItThere.Common;
using LeaveItThere.Helpers;
using System;
using System.Collections.Generic;
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
                pair.RemoteInteractable.SetItemColor(Settings.PlacedItemTint.Value);
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
            remoteInteractable.SetItemColor(Settings.PlacedItemTint.Value);
            if (lootItem.Item.IsContainer)
            {
                remoteInteractable.Actions.Add(PlacementController.GetRemoteOpenItemAction(lootItem));
            }
            remoteInteractable.Actions.Add(PlacementController.GetDemolishItemAction(remoteInteractable));

            remoteAccessObj.transform.position = position;
            remoteAccessObj.transform.rotation = rotation;
            remoteAccessObj.transform.localScale = remoteAccessObj.transform.localScale * 0.99f;

            return remoteInteractable;
        }

        public void SetItemColor(Color color)
        {
            var renderers = GetComponentsInChildren<MeshRenderer>();
            foreach (var renderer in renderers)
            {
                var material = renderer.material;
                if (!material.HasProperty("_Color")) continue;
                renderer.material.color = color;
            }
        }
    }
}
