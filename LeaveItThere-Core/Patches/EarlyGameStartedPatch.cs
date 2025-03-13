using Comfort.Common;
using EFT;
using HarmonyLib;
using LeaveItThere.Components;
using LeaveItThere.Fika;
using SPT.Reflection.Patching;
using System.Reflection;

namespace LeaveItThere.Patches
{
    internal class EarlyGameStartedPatch : ModulePatch
    {
#if FIKA_COMPATIBLE
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
#else
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
#endif
    }
}
