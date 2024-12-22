using BepInEx;
using BepInEx.Logging;
using EFT.UI;
using Newtonsoft.Json;
using LeaveItThere.Common;
using LeaveItThere.Helpers;
using LeaveItThere.Patches;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Configuration.Assemblies;

namespace LeaveItThere
{
    [BepInPlugin("Jehree.LeaveItThere", "LeaveItThere", "1.1.0")]
    public class Plugin : BaseUnityPlugin
    {
        public static ManualLogSource LogSource;
        private static string _assemblyPath = Assembly.GetExecutingAssembly().Location;
        public static string AssemblyFolderPath = Path.GetDirectoryName(_assemblyPath);
        private static string _itemFilterPath = Path.Combine(AssemblyFolderPath, "placeable_item_filter.json");
        internal static ItemFilter PlaceableItemFilter { get; private set; }
        private void Awake()
        {
            LogSource = Logger;
            PlaceableItemFilter = JsonConvert.DeserializeObject<ItemFilter>(File.ReadAllText(_itemFilterPath));
            Settings.Init(Config);
            LogSource.LogWarning("Ebu is cute :3");

            new GetAvailableActionsPatch().Enable();
            new GameStartedPatch().Enable();
            new GameEndedPatch().Enable();

            ConsoleScreen.Processor.RegisterCommandGroup<ConsoleCommands>();
        }
    }
}