using LeaveItThere.Fika;
using System.Collections.Generic;

namespace LeaveItThere.Addon;

public static class AddonTools
{
    internal static Dictionary<string, int> CostOverrides = [];

    /// <summary>
    /// Override cost amount to place an item.
    /// </summary>
    /// <param name="templateId">Template ID for item to override</param>
    /// <param name="cost">New cost</param>
    public static void AddPlacementCostOverride(string templateId, int cost)
    {
        CostOverrides[templateId] = cost;
    }

    /// <summary>
    /// Returns true if Fika is not installed, or if it is and this client is the host of the raid.
    /// </summary>
    public static bool IAmHost()
    {
        return FikaBridge.IAmHost();
    }

    /// <summary>
    /// Returns profile id if Fika is not installed, or the id of the raid if it is (which will be the host's profile id).
    /// </summary>
    public static string GetRaidId()
    {
        return FikaBridge.GetRaidId();
    }
}
