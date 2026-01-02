using EFT;
using HarmonyLib;
using LeaveItThere.Components;
using LeaveItThere.Helpers;
using LeaveItThere.Models;
using SPT.Reflection.Patching;
using System.Reflection;

namespace LeaveItThere.Patches;

internal class EarlyGameStartedPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(BotsController), nameof(BotsController.SetSettings));
    }

    [PatchPostfix]
    static void PatchPrefix()
    {
        RaidSession.Instance.ServerDataPack = LITUtils.ServerRoute(Plugin.DataToClientURL, ServerDataPack.Request);
        ItemSpawner.SpawnAllPlacedItems();
    }
}

internal class EarlyGameStartedPatchFika : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(GameWorld), nameof(GameWorld.RegisterRestrictableZones));
    }

    [PatchPostfix]
    static void PatchPrefix(GameWorld __instance)
    {
        if (__instance is HideoutGameWorld) return;
        RaidSession.Instance.ServerDataPack = LITUtils.ServerRoute(Plugin.DataToClientURL, ServerDataPack.Request);
        ItemSpawner.SpawnAllPlacedItems();
    }
}

