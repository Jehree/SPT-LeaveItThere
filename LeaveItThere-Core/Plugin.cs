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
using BepInEx.Bootstrap;
using LeaveItThere.Fika;

namespace LeaveItThere
{
    [BepInPlugin("Jehree.LeaveItThere", "LeaveItThere", "1.3.0")]
    public class Plugin : BaseUnityPlugin
    {
        public static bool FikaInstalled { get; private set; }
        public static bool IAmDedicatedClient { get; private set; }

        public static ManualLogSource LogSource;
        private static string _assemblyPath = Assembly.GetExecutingAssembly().Location;
        public static string AssemblyFolderPath = Path.GetDirectoryName(_assemblyPath);
        private static string _itemFilterPath = Path.Combine(AssemblyFolderPath, "placeable_item_filter.json");
        internal static ItemFilter PlaceableItemFilter { get; private set; }
        private void Awake()
        {
            FikaInstalled = Chainloader.PluginInfos.ContainsKey("com.fika.core");
            IAmDedicatedClient = Chainloader.PluginInfos.ContainsKey("com.fika.dedicated");

            LogSource = Logger;
            PlaceableItemFilter = JsonConvert.DeserializeObject<ItemFilter>(File.ReadAllText(_itemFilterPath));
            Settings.Init(Config);
            LogSource.LogWarning("Ebu is cute :3");

            new GetAvailableActionsPatch().Enable();
            new GameStartedPatch().Enable();
            new GameEndedPatch().Enable();

            ConsoleScreen.Processor.RegisterCommandGroup<ConsoleCommands>();
        }

        private void OnEnable()
        {
            FikaInterface.InitOnPluginEnabled();
        }
    }
}
    // need a way for placed item initialization to happen when a another client places a NEW item
    // moving isn't working, placing and unplacing is syncing fine, but moving does not show it in the new location on other clients, even when placing and reclaiming the item after moving