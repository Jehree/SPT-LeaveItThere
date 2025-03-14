using EFT;
using HarmonyLib;
using LeaveItThere.Addon;
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
        private static FieldInfo _exitNameInfo;

        protected override MethodBase GetTargetMethod()
        {
            _targetClassType = PatchConstants.EftTypes.Single(targetClass =>
                !targetClass.IsInterface &&
                !targetClass.IsNested &&
                targetClass.GetMethods().Any(method => method.Name == "LocalRaidEnded") &&
                targetClass.GetMethods().Any(method => method.Name == "LocalRaidStarted")
            );

            MethodInfo targetMethod = AccessTools.Method(_targetClassType.GetTypeInfo(), "LocalRaidEnded");

            _exitNameInfo = targetMethod.GetParameters()[1].ParameterType.GetField("exitName");

            return targetMethod;
        }

        // LocalRaidSettings settings, GClass1924 results, GClass1301[] lostInsuredItems, Dictionary<string, GClass1301[]> transferItems
        [PatchPrefix]
        static void Prefix(LocalRaidSettings settings, object results, ref object lostInsuredItems, object transferItems)
        {
            LITStaticEvents.InvokeOnRaidEnd(settings, results, lostInsuredItems, transferItems, _exitNameInfo.GetValue(results) as string);

            LITSession session = LITSession.Instance;
            lostInsuredItems = ItemHelper.RemoveLostInsuredItemsByIds(lostInsuredItems as object[], session.GetPlacedItemInstanceIds());

            if (FikaBridge.IAmHost())
            {
                session.SendPlacedItemDataToServer();
            }

            session.DestroyAllFakeItems();
        }
    }
}
