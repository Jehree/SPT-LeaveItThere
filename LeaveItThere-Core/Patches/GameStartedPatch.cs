using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using EFT.UI;
using HarmonyLib;
using LeaveItThere.Common;
using LeaveItThere.Components;
using LeaveItThere.Helpers;
using SPT.Reflection.Patching;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

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
            ModSession.CreateNewSession();
            ObjectMover.CreateNewObjectMover();
        }
    }
}
