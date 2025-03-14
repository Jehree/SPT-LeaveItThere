using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using EFT.UI;
using LeaveItThere.Common;
using LeaveItThere.Fika;
using LeaveItThere.Helpers;
using LeaveItThere.Patches;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;

namespace LeaveItThere
{
    [BepInDependency("com.fika.core", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInPlugin("Jehree.LeaveItThere", "LeaveItThere", "1.6.0")]
    public class Plugin : BaseUnityPlugin
    {
        public static bool FikaInstalled { get; private set; }

        public const string DataToServerURL = "/jehree/pip/data_to_server";
        public const string DataToClientURL = "/jehree/pip/data_to_client";

        public static ManualLogSource LogSource;
        private static string _assemblyPath = Assembly.GetExecutingAssembly().Location;
        public static string AssemblyFolderPath = Path.GetDirectoryName(_assemblyPath);
        private static string _itemFilterPath = Path.Combine(AssemblyFolderPath, "placeable_item_filter.json");
        internal static ItemFilter PlaceableItemFilter { get; private set; }
        private void Awake()
        {
            FikaInstalled = Chainloader.PluginInfos.ContainsKey("com.fika.core");
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

            if (FikaInstalled)
            {
                new EarlyGameStartedPatchFika().Enable();
            }
            else
            {
                new EarlyGameStartedPatch().Enable();
            }
            new GetAvailableActionsPatch().Enable();
            new GameEndedPatch().Enable();
            new InteractionsChangedHandlerPatch().Enable();
            new LootExperiencePatch().Enable();

            ConsoleScreen.Processor.RegisterCommandGroup<ConsoleCommands>();

            TryInitFikaAssembly();
        }

        private void OnEnable()
        {
            FikaBridge.PluginEnable();
        }

        void TryInitFikaAssembly()
        {
            if (!FikaInstalled) return;

            Assembly fikaModuleAssembly = Assembly.Load("LeaveItThere-FikaModule");
            Type main = fikaModuleAssembly.GetType("LeaveItThere.FikaModule.Main");
            MethodInfo init = main.GetMethod("Init");

            init.Invoke(main, null);
        }

        public static void DebugLog(string message)
        {
#if DEBUG
            LogSource.LogError($"[Debug Log]: {message}");
#endif
        }
    }
}