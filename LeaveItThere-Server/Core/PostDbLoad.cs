using LeaveItThereServer.Models;
using LeaveItThereServer.Utils;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services;

namespace LeaveItThereServer.Core;

[Injectable(TypePriority = OnLoadOrder.PostDBModLoader + 1)]
public class PostDbLoad(
    ISptLogger<PostDbLoad> logger,
    DatabaseService databaseService,
    ModUtils modUtils)
    : IOnLoad
{
    public Task OnLoad()
    {
        ModConfig config = modUtils.ModConfig;

        if (config.RemoveInRaidRestrictions)
        {
            databaseService.GetGlobals().Configuration.RestrictionsInRaid = [];
        }

        foreach (var (_, item) in databaseService.GetItems().Where(i => i.Value.Type == "Item"))
        {
            if (config.EverythingIsDiscardable && item.Properties is not null)
            {
                item.Properties.DiscardLimit = -1;
            }

            if (
                config.RemoveBackpackRestrictions &&
                item.Parent == BaseClasses.BACKPACK &&
                item.Properties?.Grids is not null)
            {
                foreach (Grid grid in item.Properties.Grids)
                {
                    if (grid.Properties is null) continue;

                    grid.Properties.Filters =
                    [
                        new GridFilter()
                        {
                            Filter = [BaseClasses.ITEM],
                            ExcludedFilter = []
                        }
                    ];
                }
            }
        }

        return Task.CompletedTask;
    }
}
