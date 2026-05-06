using Comfort.Common;
using EFT;
using LeaveItThere.Components;
using SPT.Reflection.Patching;
using System.Reflection;

namespace LeaveItThere.Patches;

internal class LootExperiencePatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        // args: (Item item, bool grab)
        return typeof(LocationStatisticsCollectorAbstractClass).GetMethod(nameof(LocationStatisticsCollectorAbstractClass.method_13));
    }

    [PatchPrefix]
    public static bool Prefix()
    {
        if (!Singleton<GameWorld>.Instantiated) return true;
        return RaidSession.Instance.LootExperienceEnabled;
    }
}
