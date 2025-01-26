using EFT;
using EFT.UI.Matchmaker;
using HarmonyLib;
using LeaveItThere.Components;
using SPT.Reflection.Patching;
using System.Reflection;

namespace LeaveItThere.Patches
{
    internal class GameStartedPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(GameWorld).GetMethod(nameof(GameWorld.OnGameStarted));
        }

        [PatchPostfix]
        static void PatchPrefix()
        {
            Plugin.LogSource.LogError("happened"); 
            LITSession.CreateNewModSession();
            ObjectMover.CreateNewObjectMover();
        }
    }
}
