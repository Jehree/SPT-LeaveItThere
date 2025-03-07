﻿using EFT;
using HarmonyLib;
using LeaveItThere.Components;
using LeaveItThere.Fika;
using SPT.Reflection.Patching;
using System.Reflection;

namespace LeaveItThere.Patches
{
    internal class GameStartedPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BotsController), nameof(BotsController.SetSettings));
        }

        [PatchPostfix]
        static void PatchPrefix()
        {
            Plugin.DebugLog("happened");
            LITSession.CreateNewModSession();
            ObjectMover.CreateNewObjectMover();
        }
    }
}
