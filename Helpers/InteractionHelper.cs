using Comfort.Common;
using EFT;
using EFT.Interactive;
using EFT.InventoryLogic;
using EFT.UI;
using HarmonyLib;
using LeaveItThere.Common;
using LeaveItThere.Components;
using System.Collections.Generic;
using System.Reflection;

namespace LeaveItThere.Helpers
{
    public static class InteractionHelper
    {
        private static GamePlayerOwner _owner;

        public static MethodInfo GetInteractiveActionsMethodInfo<TIneractive>()
        {
            
            return AccessTools.FirstMethod(
                typeof(GetActionsClass),
                method =>
                method.GetParameters()[0].Name == "owner" &&
                method.GetParameters()[1].ParameterType == typeof(TIneractive)
            );
        }

        public static void RefreshPrompt()
        {
            _owner.ClearInteractionState();
        }

        public static void NotificationLong(string message)
        {
            NotificationManagerClass.DisplayMessageNotification(message, EFT.Communications.ENotificationDurationType.Long);
        }
    }
}
