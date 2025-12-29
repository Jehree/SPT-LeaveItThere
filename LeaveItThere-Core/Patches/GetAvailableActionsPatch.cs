using EFT;
using EFT.Interactive;
using HarmonyLib;
using JetBrains.Annotations;
using LeaveItThere.Common;
using LeaveItThere.Components;
using SPT.Reflection.Patching;
using System.Reflection;

namespace LeaveItThere.Patches;

internal class GetAvailableActionsPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.FirstMethod(typeof(GetActionsClass), method => method.Name == nameof(GetActionsClass.GetAvailableActions) && method.GetParameters()[0].Name == "owner");
    }

    [PatchPrefix]
    static bool PatchPrefix(GamePlayerOwner owner, [CanBeNull] GInterface177 interactive, ref ActionsReturnClass __result)
    {
        if (interactive is not FakeItem) return true;

        __result = (interactive as FakeItem).InteractionContainer.ActionsReturnClass;

        return false;
    }

    [PatchPostfix]
    static void PatchPostfix(GamePlayerOwner owner, [CanBeNull] GInterface177 interactive, ref ActionsReturnClass __result)
    {
        if (interactive is not LootItem) return;
        if (interactive is Corpse) return;

        CustomInteraction placeAction = new FakeItem.PlaceItemInteraction(interactive as LootItem);
        __result.Actions.Add(placeAction.ActionsTypesClass);
    }
}