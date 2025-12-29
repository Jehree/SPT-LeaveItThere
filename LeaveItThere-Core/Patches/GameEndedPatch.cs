using EFT;
using HarmonyLib;
using LeaveItThere.Components;
using LeaveItThere.Fika;
using LeaveItThere.Helpers;
using LeaveItThere.Models;
using SPT.Reflection.Patching;
using System.Collections.Generic;
using System.Reflection;

namespace LeaveItThere.Patches;

internal class GameEndedPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(Class308), nameof(Class308.LocalRaidEnded));
    }

    [PatchPrefix]
    static void PatchPrefix(LocalRaidSettings settings, RaidEndDescriptorClass results, ref FlatItemsDataClass[] lostInsuredItems, Dictionary<string, FlatItemsDataClass[]> transferItems)
    {
        lostInsuredItems = ItemHelper.RemoveLostInsuredItemsByIds(lostInsuredItems, RaidSession.Instance.GetPlacedItemInstanceIds());

        if (FikaBridge.IAmHost())
        {
            LITUtils.ServerRoute(Plugin.DataToServerURL, new ServerDataPack(RaidSession.Instance.FakeItems));
        }

        RaidSession.Instance.DestroyAllFakeItems();
    }
}