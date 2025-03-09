using Comfort.Common;
using EFT;
using HarmonyLib;
using LeaveItThere.Components;
using LeaveItThere.Fika;
using SPT.Reflection.Patching;
using System.Reflection;

namespace LeaveItThere.Patches
{
    // This patch is only enabled when Fika is installed
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
            ObjectMover.CreateNewObjectMover();
        }
    }

    // This patch is only enabled when Fika is NOT installed
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
            ObjectMover.CreateNewObjectMover();
        }
    }
}
