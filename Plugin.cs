using BepInEx;
using BepInEx.Logging;
using PersistentCaches.Helpers;
using PersistentCaches.Patches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersistentCaches
{
    [BepInPlugin("Jehree.PersistentCaches", "PersistentCaches", "1.0.0")]
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
        }
    }
}
