using EFT.Interactive;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Comfort.Common;
using InteractableInteractionsAPI.Common;
using PersistentCaches.Helpers;
using UnityEngine;
using EFT.InventoryLogic;
using PersistentCaches.Components;
using static RootMotion.FinalIK.InteractionTrigger.Range;
using System.ComponentModel;

namespace PersistentCaches.Patches
{
    internal class GetAvailableActionsPatch : ModulePatch
    {
        private static MethodInfo _getLootItemActions;
        private static FieldInfo _staticLootIdField;
        private const string CRATE_ID = "5811ce772459770e9e5f9532";
        private static GamePlayerOwner _owner;

        protected override MethodBase GetTargetMethod()
        {
            _getLootItemActions = InteractionHelper.GetInteractiveActionsMethodInfo<LootItem>();
            _staticLootIdField = AccessTools.Field(typeof(StaticLoot), "_id");

            return AccessTools.FirstMethod(typeof(GetActionsClass), method => method.Name == nameof(GetActionsClass.GetAvailableActions) && method.GetParameters()[0].Name == "owner");
        }

        [PatchPrefix]
        public static bool PatchPrefix(object[] __args, ref ActionsReturnClass __result)
        {
            var player = Singleton<GameWorld>.Instance.MainPlayer;
            var owner = __args[0] as GamePlayerOwner;
            var interactive = __args[1]; // as GInterface139 as of SPT 3.10
            if (_owner == null)
            {
                _owner = owner;
            }

            if (!WeCareAboutInteractive(interactive)) return true;

            if (interactive is LootItem)
            {
                __result = HandleLootItemInteractive(interactive, owner);
            }

            if (interactive is RemoteInteractableComponent)
            {
                var component = interactive as RemoteInteractableComponent;
                __result = new ActionsReturnClass { Actions = CustomInteractionAction.GetActionsTypesClassList(component.Actions) };
            }

            return false;
        }

        private static ActionsReturnClass HandleLootItemInteractive(object interactive, GamePlayerOwner owner)
        {
            var lootItem = interactive as LootItem;

            var action = new CustomInteractionAction("Place Cache", false, () =>
            {
                PlaceCache(lootItem as ObservedLootItem);
            });

            List<ActionsTypesClass> actions = new List<ActionsTypesClass>();
            actions.AddRange(InteractionHelper.GetVanillaInteractionActions<LootItem>(owner, interactive, _getLootItemActions));
            actions.Add(action.GetActionsTypesClass());

            return new ActionsReturnClass { Actions = actions };
        }

        private static bool WeCareAboutInteractive(object interactive)
        {
            if (interactive is LootItem) return true;
            if (interactive is RemoteInteractableComponent) return true;
            return false;
        }

        public static void PlaceCache(ObservedLootItem lootItem)
        {
            Vector3 cachePosition = lootItem.gameObject.transform.position;
            lootItem.gameObject.transform.position = new Vector3(0, -99999, 0);

            RemoteInteractableComponent remoteInteractableComponent = CreateRemoteCacheAccessObject(cachePosition, lootItem.gameObject);

            var session = PersistentCachesSession.GetSession();
            session.AddPair(lootItem, remoteInteractableComponent, cachePosition);
        }

        public static RemoteInteractableComponent CreateRemoteCacheAccessObject(Vector3 position, GameObject lootItemObject)
        {
            GameObject remoteAccessObj = GameObject.Instantiate(lootItemObject);
            ObservedLootItem lootItem = lootItemObject.GetComponent<ObservedLootItem>();

            ObservedLootItem componentWhoLivedComeToDie = remoteAccessObj.GetComponent<ObservedLootItem>();
            if (componentWhoLivedComeToDie != null)
            {
                GameObject.Destroy(componentWhoLivedComeToDie); //hehe
            }

            RemoteInteractableComponent remoteInteractableComponent = remoteAccessObj.AddComponent<RemoteInteractableComponent>();
            CustomInteractionAction openCacheAction = GetRemoteOpenCacheAction(lootItem);
            remoteInteractableComponent.Actions.Add(openCacheAction);
            remoteInteractableComponent.Actions.Add(new CustomInteractionAction("Demolish Cache", false, () => { DemolishCache(remoteInteractableComponent); }));

            remoteAccessObj.transform.position = position;

            return remoteInteractableComponent;
        }

        public static CustomInteractionAction GetRemoteOpenCacheAction(LootItem lootItem)
        {
            GetActionsClass.Class1612 @class = new GetActionsClass.Class1612();
            @class.rootItem = lootItem.Item;
            @class.owner = _owner;
            @class.lootItemLastOwner = lootItem.LastOwner;
            @class.lootItemOwner = lootItem.ItemOwner;
            @class.controller = @class.owner.Player.InventoryController;

            return new CustomInteractionAction("Open Cache", false, @class.method_3);
        }

        public static void DemolishCache(RemoteInteractableComponent remoteInteractableComponent)
        {
            var session = PersistentCachesSession.GetSession();
            var pair = session.GetPairOrNull(remoteInteractableComponent);

            remoteInteractableComponent.gameObject.transform.position = new Vector3(0, -9999, 0);
            pair.LootItem.gameObject.transform.position = pair.CachePosition;
            GameObject.Destroy(remoteInteractableComponent.gameObject);
            session.DeletePair(pair);
        }
    }
}
