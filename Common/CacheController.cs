using EFT.Interactive;
using InteractableInteractionsAPI.Common;
using PersistentCaches.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static UnityEngine.UI.GridLayoutGroup;
using UnityEngine;
using Comfort.Common;
using EFT;

namespace PersistentCaches.Common
{
    internal class CacheController
    {
        public static void PlaceCache(ObservedLootItem lootItem) 
        {
            var session = PersistentCachesSession.GetSession();
            Vector3 cachePosition = lootItem.gameObject.transform.position;
            lootItem.gameObject.transform.position = new Vector3(0, -99999, 0);

            RemoteInteractableComponent remoteInteractableComponent = GetOrCreateRemoteAccessComponent(session.GetPairOrNull(lootItem), cachePosition, lootItem);
            session.AddOrUpdatePair(lootItem, remoteInteractableComponent, cachePosition, true);
        }

        public static RemoteInteractableComponent GetOrCreateRemoteAccessComponent(CacheObjectPair pair, Vector3 cachePosition, ObservedLootItem lootItem)
        {
            if (pair == null)
            {
                return CreateNewRemoteCacheAccessObject(cachePosition, lootItem.gameObject.transform.rotation, lootItem);
            }
            else
            {
                pair.RemoteInteractableComponent.gameObject.transform.position = cachePosition;
                pair.RemoteInteractableComponent.gameObject.transform.rotation = lootItem.gameObject.transform.rotation;
                return pair.RemoteInteractableComponent;
            }
        }

        private static RemoteInteractableComponent CreateNewRemoteCacheAccessObject(Vector3 position, Quaternion rotation, ObservedLootItem lootItem)
        {
            var x = lootItem.ItemOwner.MainStorage[0].ContainedItems.Keys.ToList();

            GameObject remoteAccessObj = GameObject.Instantiate(lootItem.gameObject);

            ObservedLootItem componentWhoLivedComeToDie = remoteAccessObj.GetComponent<ObservedLootItem>();
            if (componentWhoLivedComeToDie != null)
            {
                GameObject.Destroy(componentWhoLivedComeToDie); //hehe
            }

            MeshRenderer[] renderers = remoteAccessObj.GetComponentsInChildren<MeshRenderer>();

            foreach (var renderer in renderers)
            {
                var material = renderer.material;
                if (!material.HasProperty("_Color")) continue;

                var originalColor = renderer.material.color;
                renderer.material.color = Color.yellow;
            }

            RemoteInteractableComponent remoteInteractableComponent = remoteAccessObj.AddComponent<RemoteInteractableComponent>();
            remoteInteractableComponent.Actions.Add(GetRemoteOpenCacheAction(lootItem));
            remoteInteractableComponent.Actions.Add(GetDemolishCacheAction(remoteInteractableComponent));

            remoteAccessObj.transform.position = position;
            remoteAccessObj.transform.rotation = rotation;
            remoteAccessObj.transform.localScale = remoteAccessObj.transform.localScale * 0.95f;

            return remoteInteractableComponent;
        }

        public static CustomInteractionAction GetRemoteOpenCacheAction(LootItem lootItem)
        {
            GetActionsClass.Class1612 @class = new GetActionsClass.Class1612();
            @class.rootItem = lootItem.Item;
            @class.owner = Singleton<GameWorld>.Instance.MainPlayer.gameObject.GetComponent<GamePlayerOwner>();
            @class.lootItemLastOwner = lootItem.LastOwner;
            @class.lootItemOwner = lootItem.ItemOwner;
            @class.controller = @class.owner.Player.InventoryController;

            return new CustomInteractionAction("Open Cache", false, @class.method_3);
        }

        public static CustomInteractionAction GetDemolishCacheAction(RemoteInteractableComponent remoteInteractableComponent)
        {
            return new CustomInteractionAction(
                "Demolish Cache",
                false,
                () =>
                {
                    var session = PersistentCachesSession.GetSession();
                    var pair = session.GetPairOrNull(remoteInteractableComponent);

                    remoteInteractableComponent.gameObject.transform.position = new Vector3(0, -9999, 0);
                    pair.LootItem.gameObject.transform.position = pair.CachePosition;
                    pair.LootItem.gameObject.transform.rotation = remoteInteractableComponent.gameObject.transform.rotation;
                    pair.Placed = false;
                    session.PlacedChachesCount--;
                }
            );
        }
    }
}
