using EFT;
using HarmonyLib;
using LeaveItThere.Components;
using LeaveItThere.Helpers;
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

        ItemSpawner.SpawnAllPlacedItems();
    }
}