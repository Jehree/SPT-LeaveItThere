using EFT;
using HarmonyLib;
using LeaveItThere.Components;
using LeaveItThere.Fika;
using LeaveItThere.Helpers;
using SPT.Reflection.Patching;
using SPT.Reflection.Utils;
using System;
using System.Linq;
using System.Reflection;

namespace LeaveItThere.Patches
{
    internal class GameEndedPatch : ModulePatch
    {
        private static Type _targetClassType;

        protected override MethodBase GetTargetMethod()
        {
            _targetClassType = PatchConstants.EftTypes.Single(targetClass =>
                !targetClass.IsInterface &&
                !targetClass.IsNested &&
                targetClass.GetMethods().Any(method => method.Name == "LocalRaidEnded") &&
                targetClass.GetMethods().Any(method => method.Name == "ReceiveInsurancePrices")
            );

            return AccessTools.Method(_targetClassType.GetTypeInfo(), "LocalRaidEnded");
        }

        // LocalRaidSettings settings, GClass1924 results, GClass1301[] lostInsuredItems, Dictionary<string, GClass1301[]> transferItems
        [PatchPrefix]
        static void Prefix(LocalRaidSettings settings, object results, ref object lostInsuredItems, object transferItems)
        {
            var session = ModSession.Instance;
            lostInsuredItems = ItemHelper.RemoveLostInsuredItemsByIds(lostInsuredItems as object[], session.GetPlacedItemInstanceIds());

            if (FikaInterface.IAmHost())
            {
                session.SendPlacedItemDataToServer();
            }

            session.DestroyAllRemoteObjects();
        }
    }
}
