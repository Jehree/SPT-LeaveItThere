using LeaveItThereServer.Models;
using LeaveItThereServer.Utils;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Utils;

namespace LeaveItThereServer.Core;

[Injectable]
public class CustomStaticRouter(JsonUtil jsonUtil, Callbacks callbacks)
    : StaticRouter(
        jsonUtil,
        [
            new RouteAction<ServerDataPack>(
                "/jehree/leaveitthere/data_to_server",
                async (url, info, sessionId, output) => await callbacks.OnDataToServer(url, info, sessionId)
            ),
            new RouteAction<ServerDataPack>(
                "/jehree/leaveitthere/data_to_client",
                async (url, info, sessionId, output) => await callbacks.OnDataToClient(url, info, sessionId)
            )
        ]
    )
{ }

[Injectable]
public class Callbacks(
    JsonUtil jsonUtil,
    HttpResponseUtil httpResponseUtil,
    ISptLogger<Callbacks> logger,
    ModUtils modUtils)
{
    public ValueTask<string> OnDataToServer(string url, ServerDataPack info, MongoId sessionId)
    {
        MakeBackup(info.ProfileId);

        string path = GetProfileDataPath(info.ProfileId, info.MapId);
        File.WriteAllText(path, jsonUtil.Serialize(info, true));

        return new ValueTask<string>(httpResponseUtil.NullResponse());
    }

    public ValueTask<string> OnDataToClient(string url, ServerDataPack info, MongoId sessionId)
    {
        string path = GetProfileDataPath(info.ProfileId, info.MapId);

        string? jsonData;

        jsonData = File.Exists(path)
            ? File.ReadAllText(path)
            : jsonUtil.Serialize(ServerDataPack.GetEmpty(info.ProfileId, info.MapId));

        return new ValueTask<string>(jsonData ?? httpResponseUtil.NullResponse());
    }

    public string GetProfileFolderPath(string profileId)
    {
        string profileFolderName = modUtils.ModConfig.GlobalItemDataProfile
            ? "global"
            : profileId;

        string folderPath = Path.Combine(modUtils.PathToProfilesFolder, "LeaveItThere-ItemData", profileFolderName);
        Directory.CreateDirectory(folderPath);

        return folderPath;
    }

    public string GetProfileDataPath(string profileId, string mapId)
    {
        string mapName = mapId.ToLower();

        if (mapId == "factory4_day" || mapId == "factory4_night")
        {
            mapName = "factory";
        }
        else if (mapId == "sandbox_high")
        {
            mapName = "sandbox";
        }

        string folderPath = GetProfileFolderPath(profileId);

        return Path.Combine(folderPath, $"{mapName}.json");
    }

    public void MakeBackup(string profileId)
    {
        string profileFolderPath = GetProfileFolderPath(profileId);
        string backupsFolderPath = Path.Combine(profileFolderPath, "backups");
        string thisBackupFolderPath = Path.Combine(backupsFolderPath, GetTimestamp());

        modUtils.CopyAllFilesInDirectory(profileFolderPath, thisBackupFolderPath);
        CullOldBackups(backupsFolderPath);
    }

    public void CullOldBackups(string backupsFolderPath)
    {
        string[] folders = Directory.GetDirectories(backupsFolderPath);

        if (folders.Length < 2) return;
        if (folders.Length <= modUtils.ModConfig.MaxProfileBackupCount) return;

        string selectedFolder = folders[0];

        foreach (string folder in folders)
        {
            DateTime thisFolderCreationTime = File.GetCreationTime(folder);
            DateTime selectedFolderCreationTime = File.GetCreationTime(selectedFolder);

            if (DateTime.Compare(thisFolderCreationTime, selectedFolderCreationTime) < 0)
            {
                selectedFolder = folder;
            }
        }

        Directory.Delete(selectedFolder, true);

        // recursively call self again to keep culling until no more than max in config
        CullOldBackups(backupsFolderPath);
    }

    public string GetTimestamp()
    {
        return DateTime.Now.ToString().Replace(" ", "_").Replace(":", "-").Replace("/", "-");
    }
}