using Comfort.Common;
using EFT;
using EFT.Interactive;
using InteractableInteractionsAPI.Common;
using PersistentItemPlacement.Common;
using PersistentItemPlacement.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PersistentItemPlacement.Components
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
                pair.RemoteInteractableComponent.gameObject.transform.position = position;
                pair.RemoteInteractableComponent.gameObject.transform.rotation = lootItem.gameObject.transform.rotation;
                pair.RemoteInteractableComponent.SetItemColor(Settings.PlacedItemTint.Value);
                return pair.RemoteInteractableComponent;
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

            RemoteInteractable remoteInteractableComponent = remoteAccessObj.AddComponent<RemoteInteractable>();
            remoteInteractableComponent.SetItemColor(Settings.PlacedItemTint.Value);
            if (lootItem.Item.IsContainer)
            {
                remoteInteractableComponent.Actions.Add(PlacementController.GetRemoteOpenItemAction(lootItem));
            }
            remoteInteractableComponent.Actions.Add(PlacementController.GetDemolishItemAction(remoteInteractableComponent));

            remoteAccessObj.transform.position = position;
            remoteAccessObj.transform.rotation = rotation;
            remoteAccessObj.transform.localScale = remoteAccessObj.transform.localScale * 0.99f;

            return remoteInteractableComponent;
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
