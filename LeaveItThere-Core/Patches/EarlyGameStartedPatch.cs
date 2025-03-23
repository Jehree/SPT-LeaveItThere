using EFT;
using HarmonyLib;
using LeaveItThere.Components;
using LeaveItThere.CustomUI;
using SPT.Reflection.Patching;
using System.Reflection;

namespace LeaveItThere.Patches
{
    internal class EarlyGameStartedPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BotsController), nameof(BotsController.SetSettings));
        }

        [PatchPostfix]
        static void PatchPrefix()
        {
            LITSession.CreateNewModSession();
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

            LITSession.CreateNewModSession();
            MoveModeUI.Instance.SetActive(false);
        }
    }
}
