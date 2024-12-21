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

namespace LeaveItThere
{
    [BepInPlugin("Jehree.LeaveItThere", "LeaveItThere", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        public static ManualLogSource LogSource;
        private void Awake()
        {
            Settings.Init(Config);
            LogSource = Logger;
            LogSource.LogWarning("Ebu is cute :3");

            new GetAvailableActionsPatch().Enable();
            new GameStartedPatch().Enable();
            new GameEndedPatch().Enable();

            ConsoleScreen.Processor.RegisterCommandGroup<ConsoleCommands>();
        }
    }
}