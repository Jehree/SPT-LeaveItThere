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
using PersistentItemPlacement.Helpers;
using UnityEngine;
using EFT.InventoryLogic;
using PersistentItemPlacement.Components;
using static RootMotion.FinalIK.InteractionTrigger.Range;
using System.ComponentModel;
using PersistentItemPlacement.Common;

namespace PersistentItemPlacement.Patches
{
    internal class GetAvailableActionsPatch : ModulePatch
    {
        private static MethodInfo _getLootItemActions;
        private static FieldInfo _staticLootIdField;
        private const string CRATE_ID = "5811ce772459770e9e5f9532";

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
            var placeAction = PlacementController.GetPlaceItemAction(lootItem);

            List<ActionsTypesClass> actions = new List<ActionsTypesClass>();
            actions.AddRange(InteractionHelper.GetVanillaInteractionActions<LootItem>(owner, interactive, _getLootItemActions));
            if (!actions.Any())
            {
                actions.Add(new CustomInteractionAction("No Space", true, null).GetActionsTypesClass());
            }
            actions.Add(placeAction.GetActionsTypesClass());

            return new ActionsReturnClass { Actions = actions };
        }

        private static bool WeCareAboutInteractive(object interactive)
        {
            if (interactive is LootItem) return true;
            if (interactive is RemoteInteractableComponent) return true;
            return false;
        }
    }
}
