using EFT;
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

        [PatchPrefix]
        static void PatchPrefix()
        {
            LITSession.CreateNewModSession();
            ObjectMover.CreateNewObjectMover();
        }
    }
}
