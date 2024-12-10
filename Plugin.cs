using BepInEx;
using BepInEx.Logging;
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

        // BaseUnityPlugin inherits MonoBehaviour, so you can use base unity functions like Awake() and Update()
        private void Awake()
        {
            LogSource = Logger;

            new GetAvailableActionsPatch().Enable();
            new GameStartedPatch().Enable();
        }
    }
}
