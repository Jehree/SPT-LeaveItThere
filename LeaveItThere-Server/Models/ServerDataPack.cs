using SPTarkov.Server.Core.Models.Utils;

namespace LeaveItThereServer.Models;

public class ServerDataPack : IRequestData
{
    public required string ProfileId { get; set; }
    public required string MapId { get; set; }
    public required object? ItemTemplates { get; set; }

    public static ServerDataPack GetEmpty(string profileId, string mapId)
    {
        return new()
        {
            ProfileId = profileId,
            MapId = mapId,
            ItemTemplates = default
        };
    }
}