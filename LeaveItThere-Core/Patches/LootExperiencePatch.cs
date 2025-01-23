using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using LeaveItThere.Components;
using SPT.Reflection.Patching;
using System.Linq;
using System.Reflection;

namespace LeaveItThere.Patches
{
    internal class LootExperiencePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            //return typeof(LocationStatisticsCollectorAbstractClass).GetMethod(nameof(LocationStatisticsCollectorAbstractClass.method_12));
            return typeof(LocationStatisticsCollectorAbstractClass).GetMethods().FirstOrDefault(method =>
                method.GetParameters().Count() == 2 &&
                method.GetParameters()[0].Name == "item" &&
                method.GetParameters()[0].ParameterType == typeof(Item) &&
                method.GetParameters()[1].Name == "grab" &&
                method.GetParameters()[1].ParameterType == typeof(bool)
            );
        }

        [PatchPrefix]
        public static bool Prefix()
        {
            if (!Singleton<GameWorld>.Instantiated) return true;
            return LITSession.Instance.LootExperienceEnabled;
        }
    }
}
