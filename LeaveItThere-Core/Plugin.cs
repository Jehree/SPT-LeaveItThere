using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using EFT.UI;
using LeaveItThere.Common;
using LeaveItThere.Fika;
using LeaveItThere.Helpers;
using LeaveItThere.Patches;
using Newtonsoft.Json;
using System.IO;
using System.Reflection;

namespace LeaveItThere
{
    [BepInDependency("com.fika.core", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInPlugin("Jehree.LeaveItThere", "LeaveItThere", "1.3.1")]
    public class Plugin : BaseUnityPlugin
    {
        public const string DataToServerURL = "/jehree/pip/data_to_server";
        public const string DataToClientURL = "/jehree/pip/data_to_client";

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

            if (File.Exists(_itemFilterPath))
            {
                PlaceableItemFilter = JsonConvert.DeserializeObject<ItemFilter>(File.ReadAllText(_itemFilterPath));
            }
            else
            {
                PlaceableItemFilter = new ItemFilter();
                string json = JsonConvert.SerializeObject(PlaceableItemFilter);
                File.WriteAllText(_itemFilterPath, json);
            }

            Settings.Init(Config);
            LogSource.LogInfo("Ebu is cute :3");

            new GetAvailableActionsPatch().Enable();
            new GameStartedPatch().Enable();
            new GameEndedPatch().Enable();
            new InteractionsChangedHandlerPatch().Enable();

            ConsoleScreen.Processor.RegisterCommandGroup<ConsoleCommands>();
        }

        private void OnEnable()
        {
            FikaInterface.InitOnPluginEnabled();
        }

        // TODO: LootExperience getter patch for Item's, add items spawned by mod to a list of items that will have their getter zeroed so they don't give loot exp.
        // TODO: Remove unplaced FakeItem caching. It is unnecessary and causes bugs.
    }
}