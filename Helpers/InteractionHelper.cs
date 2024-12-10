using EFT.Interactive;
using EFT;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PersistentCaches.Helpers
{
    public static class InteractionHelper
    {
        public static List<ActionsTypesClass> GetVanillaInteractionActions<TInteractive>(GamePlayerOwner gamePlayerOwner, object interactive, MethodInfo methodInfo)
        {
            object[] args = new object[2];
            args[0] = gamePlayerOwner;
            args[1] = interactive;

            if (!(interactive is TInteractive))
            {
                return new List<ActionsTypesClass>();
            }

            List<ActionsTypesClass> vanillaExfilActions = ((ActionsReturnClass)methodInfo.Invoke(null, args))?.Actions;

            return vanillaExfilActions ?? new List<ActionsTypesClass>();
        }

        public static MethodInfo GetInteractiveActionsMethodInfo<TIneractive>()
        {
            return AccessTools.FirstMethod(
                typeof(GetActionsClass),
                method =>
                method.GetParameters()[0].Name == "owner" &&
                method.GetParameters()[1].ParameterType == typeof(TIneractive)
            );
        }
    }
}
