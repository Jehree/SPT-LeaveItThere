using LeaveItThereServer.Models;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers;
using System.Reflection;

namespace LeaveItThereServer.Utils;

[Injectable]
public class ModUtils(ModHelper modHelper)
{
    private string? _pathToMod = null;
    public string PathToMod
    {
        get
        {
            _pathToMod ??= modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly());
            return _pathToMod;
        }
    }

    private string? _pathToProfilesFolder = null;
    public string PathToProfilesFolder
    {
        get
        {
            _pathToProfilesFolder ??= Path.Combine(PathToMod, "..", "..", "profiles");
            return _pathToProfilesFolder;
        }
    }

    public string PathToConfig { get; set; } = "config.json";
    private ModConfig? _modConfig = null;
    public ModConfig ModConfig
    {
        get
        {
            _modConfig ??= modHelper.GetJsonDataFromFile<ModConfig>(PathToMod, PathToConfig);
            return _modConfig;
        }
    }

    public T GetJsonDataFromFile<T>(string relativePathToData)
    {
        return modHelper.GetJsonDataFromFile<T>(PathToMod, relativePathToData);
    }

    public void CopyAllFilesInDirectory(string sourceDir, string newDir)
    {
        Directory.CreateDirectory(newDir);

        foreach (string? filePath in Directory.GetFiles(sourceDir))
        {
            string? newFilePath = Path.Combine(newDir, Path.GetFileName(filePath));
            File.Copy(filePath, newFilePath);
        }
    }
}