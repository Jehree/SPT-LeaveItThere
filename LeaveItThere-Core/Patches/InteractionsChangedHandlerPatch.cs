using EFT;
using LeaveItThere.Components;
using SPT.Reflection.Patching;
using System.Reflection;

namespace LeaveItThere.Patches
{
    internal class InteractionsChangedHandlerPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(GamePlayerOwner).GetMethod(nameof(GamePlayerOwner.InteractionsChangedHandler));
        }

        [PatchPrefix]
        static bool PatchPrefix()
        {
            return LITSession.Instance.InteractionsAllowed;
        }
    }
}
