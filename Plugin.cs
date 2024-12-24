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
    // packet needed for the server to send all clients containing "spawn this item" info
    // packet needed for changing 'placed' state
        // idea: make the ItemRemotePair class contain the id of the loot item so it can be looked up via a packet received with that id
        // packet contains id of loot item, position and rotation of placement, and 'placed' state. item on receiving client then updates the pair and places or unplaces it.
        // same exact packet could probably be used for 'move' mode