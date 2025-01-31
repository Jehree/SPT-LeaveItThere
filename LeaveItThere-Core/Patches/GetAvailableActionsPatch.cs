using EFT;
using EFT.Interactive;
using HarmonyLib;
using LeaveItThere.Common;
using LeaveItThere.Components;
using LeaveItThere.Helpers;
using SPT.Reflection.Patching;
using System.Reflection;

namespace LeaveItThere.Patches
{
    internal class GetAvailableActionsPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.FirstMethod(typeof(GetActionsClass), method => method.Name == nameof(GetActionsClass.GetAvailableActions) && method.GetParameters()[0].Name == "owner");
        }

        [PatchPrefix]
        static bool PatchPrefix(GamePlayerOwner owner, object interactive, ref ActionsReturnClass __result)
        {
            if (interactive is not FakeItem) return true;

            FakeItem fakeItem = interactive as FakeItem;
            ActionsReturnClass newResult = new ActionsReturnClass { Actions = CustomInteraction.GetActionsTypesClassList(fakeItem.Actions) };

            __result = newResult;
            return false;
        }

        [PatchPostfix]
        static void PatchPostfix(GamePlayerOwner owner, object interactive, ref ActionsReturnClass __result)
        {
            if (interactive is not LootItem) return;
            LootItem lootItem = interactive as LootItem;
            if (!LootItemIsTarget(lootItem)) return;

            CustomInteraction placeAction = new FakeItem.PlaceItemInteraction(lootItem as ObservedLootItem);
            if (ItemHelper.ItemCanBePickedUp(lootItem.Item) == false)
            {
                string interactionName = $"No Space ({lootItem.Name.Localized()})".Localized();
                __result.Actions.Insert(0, new CustomInteraction.DisabledInteraction(interactionName).GetActionsTypesClass());
            }
            __result.Actions.Add(placeAction.GetActionsTypesClass());
        }

        static bool LootItemIsTarget(LootItem lootItem)
        {
            if (Plugin.PlaceableItemFilter.WhitelistEnabled && !Plugin.PlaceableItemFilter.Whitelist.Contains(lootItem.Item.TemplateId)) return false;
            if (Plugin.PlaceableItemFilter.BlacklistEnabled && Plugin.PlaceableItemFilter.Blacklist.Contains(lootItem.Item.TemplateId)) return false;
            if (lootItem is Corpse) return false;

            if (Settings.MinimumCostItemsArePlaceable.Value) return true;

            int cost = ItemHelper.GetItemCost(lootItem.Item);
            return cost > Settings.MinimumPlacementCost.Value;
        }
    }
}
 